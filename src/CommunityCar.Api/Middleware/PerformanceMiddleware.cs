using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CommunityCar.Api.Middleware;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    // Performance thresholds (in milliseconds)
    private const int WarningThreshold = 1000; // 1 second
    private const int CriticalThreshold = 5000; // 5 seconds

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        // Add performance headers to response
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-Processing-Time"))
            {
                context.Response.Headers["X-Processing-Time"] = stopwatch.ElapsedMilliseconds.ToString();
            }
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log performance metrics
            var performanceLog = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                Method = request.Method,
                Path = request.Path.ToString(),
                QueryString = request.QueryString.ToString(),
                StatusCode = context.Response.StatusCode,
                ElapsedMilliseconds = elapsedMs,
                UserId = context.User.Identity?.Name,
                UserAgent = request.Headers["User-Agent"].ToString(),
                ContentLength = context.Response.ContentLength
            };

            // Log based on performance thresholds
            if (elapsedMs >= CriticalThreshold)
            {
                _logger.LogCritical("Critical Performance Issue: {@PerformanceLog}", performanceLog);
            }
            else if (elapsedMs >= WarningThreshold)
            {
                _logger.LogWarning("Slow Request Performance: {@PerformanceLog}", performanceLog);
            }
            else
            {
                _logger.LogInformation("Request Performance: {@PerformanceLog}", performanceLog);
            }

            // Add performance metrics to response headers
            if (!context.Response.HasStarted)
            {
                context.Response.Headers["X-Performance-Time"] = elapsedMs.ToString();
                context.Response.Headers["X-Performance-Status"] = GetPerformanceStatus(elapsedMs);
            }
        }
    }

    private string GetPerformanceStatus(long elapsedMs)
    {
        if (elapsedMs >= CriticalThreshold) return "Critical";
        if (elapsedMs >= WarningThreshold) return "Warning";
        return "Good";
    }
}

public static class PerformanceMiddlewareExtensions
{
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMiddleware>();
    }
}
