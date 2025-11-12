using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CommunityCar.Api.Filters;

public class RateLimitFilter : IAsyncActionFilter
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitFilter> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitFilter(
        IDistributedCache cache,
        ILogger<RateLimitFilter> logger,
        RateLimitOptions options)
    {
        _cache = cache;
        _logger = logger;
        _options = options;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var clientIdentifier = GetClientIdentifier(context.HttpContext);
        var endpointKey = GetEndpointKey(context);

        var cacheKey = $"ratelimit:{clientIdentifier}:{endpointKey}";

        var requestCount = await GetRequestCountAsync(cacheKey);

        if (requestCount >= _options.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}",
                clientIdentifier, endpointKey);

            var retryAfter = GetRetryAfterSeconds(cacheKey);

            context.Result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests,
                Content = $"Rate limit exceeded. Try again in {retryAfter} seconds.",
                ContentType = "text/plain"
            };

            context.HttpContext.Response.Headers.Add("Retry-After", retryAfter.ToString());
            context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequests.ToString());
            context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.HttpContext.Response.Headers.Add("X-RateLimit-Reset", GetResetTime(cacheKey).ToString());

            return;
        }

        // Add rate limit headers
        var remaining = Math.Max(0, _options.MaxRequests - requestCount - 1);
        context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequests.ToString());
        context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
        context.HttpContext.Response.Headers.Add("X-RateLimit-Reset", GetResetTime(cacheKey).ToString());

        await IncrementRequestCountAsync(cacheKey);

        await next();
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get API key first
        var apiKey = context.GetApiKey();
        if (!string.IsNullOrEmpty(apiKey))
        {
            return $"apikey:{apiKey.GetHashCode()}";
        }

        // Try to get user ID
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private string GetEndpointKey(ActionExecutingContext context)
    {
        var controller = context.ActionDescriptor.RouteValues["controller"];
        var action = context.ActionDescriptor.RouteValues["action"];
        var method = context.HttpContext.Request.Method;

        return $"{method}:{controller}:{action}".ToLowerInvariant();
    }

    private async Task<int> GetRequestCountAsync(string cacheKey)
    {
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        return string.IsNullOrEmpty(cachedValue) ? 0 : int.Parse(cachedValue);
    }

    private async Task IncrementRequestCountAsync(string cacheKey)
    {
        var count = await GetRequestCountAsync(cacheKey) + 1;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.Window
        };

        await _cache.SetStringAsync(cacheKey, count.ToString(), options);
    }

    private int GetRetryAfterSeconds(string cacheKey)
    {
        // Simplified - in production, you'd calculate based on cache TTL
        return (int)_options.Window.TotalSeconds;
    }

    private long GetResetTime(string cacheKey)
    {
        // Return Unix timestamp for when the limit resets
        return DateTimeOffset.UtcNow.Add(_options.Window).ToUnixTimeSeconds();
    }
}

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public bool EnableSlidingWindow { get; set; } = false;
    public Dictionary<string, RateLimitRule> EndpointRules { get; set; } = new();
}

public class RateLimitRule
{
    public int MaxRequests { get; set; }
    public TimeSpan Window { get; set; }
    public string[]? AllowedRoles { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : Attribute
{
    public int MaxRequests { get; set; } = 100;
    public int WindowInMinutes { get; set; } = 1;

    public RateLimitAttribute(int maxRequests = 100, int windowInMinutes = 1)
    {
        MaxRequests = maxRequests;
        WindowInMinutes = windowInMinutes;
    }
}

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(
        RequestDelegate next,
        RateLimitOptions options,
        IDistributedCache cache,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _options = options;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpointKey = GetEndpointKey(context);
        var cacheKey = $"ratelimit:{clientId}:{endpointKey}";

        var requestCount = await GetRequestCountAsync(cacheKey);

        if (requestCount >= _options.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for {ClientId}", clientId);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", _options.Window.TotalSeconds.ToString());
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await IncrementRequestCountAsync(cacheKey);
        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        var apiKey = context.GetApiKey();
        if (!string.IsNullOrEmpty(apiKey))
            return $"apikey:{apiKey.GetHashCode()}";

        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        return $"ip:{context.Connection.RemoteIpAddress}";
    }

    private string GetEndpointKey(HttpContext context)
    {
        return $"{context.Request.Method}:{context.Request.Path}".ToLowerInvariant();
    }

    private async Task<int> GetRequestCountAsync(string cacheKey)
    {
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        return string.IsNullOrEmpty(cachedValue) ? 0 : int.Parse(cachedValue);
    }

    private async Task IncrementRequestCountAsync(string cacheKey)
    {
        var count = await GetRequestCountAsync(cacheKey) + 1;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.Window
        };
        await _cache.SetStringAsync(cacheKey, count.ToString(), options);
    }
}

public static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitOptions>(configuration.GetSection("RateLimiting"));
        services.AddDistributedMemoryCache(); // Or Redis, SQL Server, etc.
        return services;
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        app.UseMiddleware<RateLimitMiddleware>();
        return app;
    }
}
