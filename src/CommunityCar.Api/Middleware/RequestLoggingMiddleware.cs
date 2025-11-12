using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CommunityCar.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var response = context.Response;

        // Log incoming request
        var requestLog = new
        {
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier,
            RequestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? context.TraceIdentifier,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            UserAgent = request.Headers["User-Agent"].ToString(),
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User.Identity?.Name,
            IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false
        };

        _logger.LogInformation("Incoming Request: {@RequestLog}", requestLog);

        // Capture original response body stream
        var originalResponseBody = response.Body;
        using var responseBody = new MemoryStream();
        response.Body = responseBody;

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Log outgoing response
            var responseLog = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                RequestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? context.TraceIdentifier,
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                ContentLength = response.ContentLength,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                UserId = context.User.Identity?.Name
            };

            // Log based on status code
            if (response.StatusCode >= 500)
            {
                _logger.LogError("Server Error Response: {@ResponseLog}", responseLog);
            }
            else if (response.StatusCode >= 400)
            {
                _logger.LogWarning("Client Error Response: {@ResponseLog}", responseLog);
            }
            else
            {
                _logger.LogInformation("Successful Response: {@ResponseLog}", responseLog);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var errorLog = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier,
                RequestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? context.TraceIdentifier,
                Method = request.Method,
                Path = request.Path,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message,
                UserId = context.User.Identity?.Name
            };

            _logger.LogError(ex, "Unhandled Exception: {@ErrorLog}", errorLog);

            throw; // Re-throw to let exception handling middleware deal with it
        }
        finally
        {
            // Restore original response body stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBody);
            response.Body = originalResponseBody;
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}