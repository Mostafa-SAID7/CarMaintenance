using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CommunityCar.Api.Middleware;

public class VisitorTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<VisitorTrackingMiddleware> _logger;
    private readonly IMemoryCache _cache;

    public VisitorTrackingMiddleware(
        RequestDelegate next,
        ILogger<VisitorTrackingMiddleware> logger,
        IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var visitorId = GetOrCreateVisitorId(context);
        var sessionId = GetOrCreateSessionId(context);

        // Track visitor information
        var visitorInfo = new VisitorInfo
        {
            VisitorId = visitorId,
            SessionId = sessionId,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = request.Headers["User-Agent"].ToString(),
            Referrer = request.Headers["Referer"].ToString(),
            Language = request.Headers["Accept-Language"].ToString(),
            Path = request.Path.ToString(),
            Method = request.Method,
            Timestamp = DateTime.UtcNow,
            UserId = context.User.Identity?.Name,
            IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false
        };

        // Store visitor info in cache for analytics
        var cacheKey = $"visitor:{visitorId}:{DateTime.UtcNow.ToString("yyyyMMdd")}";
        var dailyStats = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            return new DailyVisitorStats();
        });

        dailyStats.TotalRequests++;
        if (!dailyStats.UniqueVisitors.Contains(visitorId))
        {
            dailyStats.UniqueVisitors.Add(visitorId);
        }

        if (visitorInfo.IsAuthenticated && !string.IsNullOrEmpty(visitorInfo.UserId))
        {
            if (!dailyStats.AuthenticatedUsers.Contains(visitorInfo.UserId))
            {
                dailyStats.AuthenticatedUsers.Add(visitorInfo.UserId);
            }
        }

        // Track page views
        var pageKey = $"{visitorInfo.Method}:{visitorInfo.Path}";
        if (!dailyStats.PageViews.ContainsKey(pageKey))
        {
            dailyStats.PageViews[pageKey] = 0;
        }
        dailyStats.PageViews[pageKey]++;

        // Update cache
        _cache.Set(cacheKey, dailyStats, TimeSpan.FromDays(1));

        // Add tracking headers to response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Visitor-ID"] = visitorId;
            context.Response.Headers["X-Session-ID"] = sessionId;
            return Task.CompletedTask;
        });

        // Log visitor activity (sample logging - adjust based on needs)
        if (ShouldLogVisitorActivity(visitorInfo))
        {
            _logger.LogInformation("Visitor Activity: {@VisitorInfo}", visitorInfo);
        }

        await _next(context);
    }

    private string GetOrCreateVisitorId(HttpContext context)
    {
        // Try to get from cookie first
        var visitorCookie = context.Request.Cookies["VisitorId"];
        if (!string.IsNullOrEmpty(visitorCookie))
        {
            return visitorCookie;
        }

        // Create new visitor ID based on IP + User Agent
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var combined = $"{ip}|{userAgent}";

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        var visitorId = Convert.ToBase64String(hash).Substring(0, 16);

        // Set cookie for future requests
        context.Response.Cookies.Append("VisitorId", visitorId, new CookieOptions
        {
            Expires = DateTime.UtcNow.AddYears(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });

        return visitorId;
    }

    private string GetOrCreateSessionId(HttpContext context)
    {
        // Try to get from cookie
        var sessionCookie = context.Request.Cookies["SessionId"];
        if (!string.IsNullOrEmpty(sessionCookie))
        {
            return sessionCookie;
        }

        // Create new session ID
        var sessionId = Guid.NewGuid().ToString("N");

        // Set session cookie
        context.Response.Cookies.Append("SessionId", sessionId, new CookieOptions
        {
            Expires = DateTime.UtcNow.AddHours(24),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });

        return sessionId;
    }

    private bool ShouldLogVisitorActivity(VisitorInfo info)
    {
        // Log only significant activities or errors
        // This could be enhanced with more sophisticated logic
        return info.Method != "GET" || info.Path.Contains("/api/");
    }
}

public class VisitorInfo
{
    public string VisitorId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string Referrer { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public bool IsAuthenticated { get; set; }
}

public class DailyVisitorStats
{
    public int TotalRequests { get; set; }
    public HashSet<string> UniqueVisitors { get; set; } = new();
    public HashSet<string> AuthenticatedUsers { get; set; } = new();
    public Dictionary<string, int> PageViews { get; set; } = new();
}

public static class VisitorTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UseVisitorTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<VisitorTrackingMiddleware>();
    }
}