using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CommunityCar.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IMemoryCache _cache;

    // Rate limiting configuration
    private const int MaxRequestsPerMinute = 60;
    private const int MaxRequestsPerHour = 1000;
    private readonly TimeSpan _minuteWindow = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _hourWindow = TimeSpan.FromHours(1);

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = context.Request.Path.ToString().ToLower();

        // Skip rate limiting for certain endpoints
        if (ShouldSkipRateLimiting(endpoint))
        {
            await _next(context);
            return;
        }

        // Check rate limits
        if (!IsWithinRateLimit(clientId, endpoint))
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}",
                clientId, endpoint);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers["X-RateLimit-Limit"] = MaxRequestsPerMinute.ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = 60
            });

            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get from API key first
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
        {
            return $"api:{apiKey}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return $"ip:{ipAddress}";
        }

        // Last resort - use a generic identifier
        return "unknown";
    }

    private bool ShouldSkipRateLimiting(string endpoint)
    {
        var skipEndpoints = new[]
        {
            "/health",
            "/api/localization",
            "/swagger",
            "/favicon.ico"
        };

        return skipEndpoints.Any(e => endpoint.StartsWith(e));
    }

    private bool IsWithinRateLimit(string clientId, string endpoint)
    {
        var minuteKey = $"{clientId}:minute:{DateTime.UtcNow.ToString("yyyyMMddHHmm")}";
        var hourKey = $"{clientId}:hour:{DateTime.UtcNow.ToString("yyyyMMddHH")}";

        // Check minute limit
        var minuteCount = _cache.GetOrCreate(minuteKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _minuteWindow;
            return 0;
        });

        if (minuteCount >= MaxRequestsPerMinute)
        {
            return false;
        }

        // Check hour limit
        var hourCount = _cache.GetOrCreate(hourKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _hourWindow;
            return 0;
        });

        if (hourCount >= MaxRequestsPerHour)
        {
            return false;
        }

        // Increment counters
        _cache.Set(minuteKey, minuteCount + 1, _minuteWindow);
        _cache.Set(hourKey, hourCount + 1, _hourWindow);

        // Add rate limit headers
        // Note: This would need to be set in the response after the request is processed

        return true;
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
