using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CommunityCar.Api.Middleware;

public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseCachingMiddleware> _logger;
    private readonly IMemoryCache _cache;

    // Cache configuration
    private const int DefaultCacheDuration = 300; // 5 minutes
    private const int MaxCacheSize = 100; // Maximum cached responses
    private readonly HashSet<string> _cacheableMethods = new() { "GET", "HEAD" };
    private readonly HashSet<string> _cacheableStatusCodes = new() { "200", "301", "302", "304" };

    public ResponseCachingMiddleware(
        RequestDelegate next,
        ILogger<ResponseCachingMiddleware> logger,
        IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        // Only cache GET and HEAD requests
        if (!_cacheableMethods.Contains(request.Method))
        {
            await _next(context);
            return;
        }

        // Skip caching for authenticated requests (unless explicitly allowed)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Check for cache-control header
            var cacheControl = request.Headers.CacheControl.ToString();
            if (!cacheControl.Contains("max-age") && !cacheControl.Contains("no-cache"))
            {
                await _next(context);
                return;
            }
        }

        // Generate cache key
        var cacheKey = GenerateCacheKey(context);

        // Try to get cached response
        if (_cache.TryGetValue(cacheKey, out CachedResponse? cachedResponse))
        {
            // Check if cached response is still valid
            if (IsCacheValid(cachedResponse, context))
            {
                _logger.LogDebug("Serving cached response for {Method} {Path}", request.Method, request.Path);

                // Copy cached response to current response
                await CopyCachedResponseToContext(cachedResponse, context);
                return;
            }
            else
            {
                // Remove expired cache entry
                _cache.Remove(cacheKey);
            }
        }

        // Capture the response
        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Cache the response if appropriate
        if (ShouldCacheResponse(context))
        {
            var responseContent = await GetResponseContent(responseBody);
            var cachedResponse = new CachedResponse
            {
                StatusCode = context.Response.StatusCode,
                Headers = new Dictionary<string, string>(),
                Content = responseContent,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(GetCacheDuration(context))
            };

            // Copy headers
            foreach (var header in context.Response.Headers)
            {
                cachedResponse.Headers[header.Key] = header.Value.ToString();
            }

            // Store in cache
            _cache.Set(cacheKey, cachedResponse, TimeSpan.FromSeconds(GetCacheDuration(context)));

            _logger.LogDebug("Cached response for {Method} {Path}", request.Method, request.Path);
        }

        // Copy response back to original stream
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalResponseBody);
        context.Response.Body = originalResponseBody;
    }

    private string GenerateCacheKey(HttpContext context)
    {
        var request = context.Request;
        var keyBuilder = new StringBuilder();

        // Base key components
        keyBuilder.Append(request.Method);
        keyBuilder.Append(request.Path);
        keyBuilder.Append(request.QueryString);

        // Include user ID for authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                keyBuilder.Append($"|user:{userId}");
            }
        }

        // Include culture for localized responses
        var culture = context.Request.Headers["Accept-Language"].ToString();
        if (!string.IsNullOrEmpty(culture))
        {
            keyBuilder.Append($"|culture:{culture}");
        }

        // Generate hash for consistent key length
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyBuilder.ToString()));
        return Convert.ToBase64String(hash);
    }

    private bool IsCacheValid(CachedResponse cachedResponse, HttpContext context)
    {
        // Check if cache has expired
        if (DateTime.UtcNow > cachedResponse.ExpiresAt)
        {
            return false;
        }

        // Check cache control headers from client
        var cacheControl = context.Request.Headers.CacheControl.ToString();
        if (cacheControl.Contains("no-cache") || cacheControl.Contains("no-store"))
        {
            return false;
        }

        return true;
    }

    private bool ShouldCacheResponse(HttpContext context)
    {
        var response = context.Response;

        // Only cache successful responses
        if (!_cacheableStatusCodes.Contains(response.StatusCode.ToString()))
        {
            return false;
        }

        // Don't cache if response has cache control headers that prevent caching
        var cacheControl = response.Headers.CacheControl.ToString();
        if (cacheControl.Contains("no-cache") || cacheControl.Contains("no-store") ||
            cacheControl.Contains("private"))
        {
            return false;
        }

        // Don't cache large responses
        if (response.ContentLength > 1024 * 1024) // 1MB
        {
            return false;
        }

        return true;
    }

    private int GetCacheDuration(HttpContext context)
    {
        // Check for cache control header
        var cacheControl = context.Response.Headers.CacheControl.ToString();
        if (!string.IsNullOrEmpty(cacheControl))
        {
            var maxAgeMatch = System.Text.RegularExpressions.Regex.Match(cacheControl, @"max-age=(\d+)");
            if (maxAgeMatch.Success && int.TryParse(maxAgeMatch.Groups[1].Value, out var maxAge))
            {
                return Math.Min(maxAge, 3600); // Max 1 hour
            }
        }

        // Default cache duration based on endpoint
        var path = context.Request.Path.ToString().ToLower();
        if (path.Contains("/api/localization") || path.Contains("/api/health"))
        {
            return 3600; // 1 hour for static content
        }
        else if (path.Contains("/api/dashboard"))
        {
            return 300; // 5 minutes for dashboard data
        }

        return DefaultCacheDuration;
    }

    private async Task<byte[]> GetResponseContent(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        using var memoryStream = new MemoryStream();
        await responseBody.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task CopyCachedResponseToContext(CachedResponse cachedResponse, HttpContext context)
    {
        context.Response.StatusCode = cachedResponse.StatusCode;

        // Copy headers
        foreach (var header in cachedResponse.Headers)
        {
            if (!context.Response.Headers.ContainsKey(header.Key))
            {
                context.Response.Headers[header.Key] = header.Value;
            }
        }

        // Add cache headers
        context.Response.Headers[HeaderNames.Age] = ((int)(DateTime.UtcNow - cachedResponse.CreatedAt).TotalSeconds).ToString();
        context.Response.Headers[HeaderNames.XCache] = "HIT";

        // Write content
        await context.Response.Body.WriteAsync(cachedResponse.Content);
    }
}

public class CachedResponse
{
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public static class ResponseCachingMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseCachingMiddleware>();
    }
}
