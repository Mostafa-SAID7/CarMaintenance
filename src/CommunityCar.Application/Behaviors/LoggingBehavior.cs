using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CommunityCar.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.GetCurrentUserId();
        var userName = _currentUserService.GetCurrentUserName();

        _logger.LogInformation("Handling {RequestName} for user {UserId} ({UserName})",
            requestName, userId ?? "Anonymous", userName ?? "Unknown");

        // Log request details for commands (but not sensitive data)
        if (request is ILoggableRequest loggableRequest && loggableRequest.ShouldLogRequestDetails)
        {
            var requestDetails = GetRequestDetails(request);
            _logger.LogInformation("Request details for {RequestName}: {Details}",
                requestName, requestDetails);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation("Handled {RequestName} successfully in {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            // Log response details for specific cases
            if (request is ILoggableRequest loggableRequest && loggableRequest.ShouldLogResponseDetails)
            {
                var responseDetails = GetResponseDetails(response);
                _logger.LogInformation("Response details for {RequestName}: {Details}",
                    requestName, responseDetails);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds}ms: {ErrorMessage}",
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }

    private string GetRequestDetails(TRequest request)
    {
        try
        {
            // Create a copy of the request to avoid modifying the original
            var requestCopy = JsonSerializer.Deserialize<TRequest>(JsonSerializer.Serialize(request));

            if (requestCopy is ISensitiveDataRequest sensitiveRequest)
            {
                // Remove or mask sensitive data
                sensitiveRequest.MaskSensitiveData();
            }

            return JsonSerializer.Serialize(requestCopy, new JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 3 // Prevent deep serialization
            });
        }
        catch
        {
            return "[Unable to serialize request]";
        }
    }

    private string GetResponseDetails(TResponse response)
    {
        try
        {
            if (response is ISensitiveDataResponse sensitiveResponse)
            {
                return "[Response contains sensitive data - not logged]";
            }

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 2
            });
        }
        catch
        {
            return "[Unable to serialize response]";
        }
    }
}

public interface ILoggableRequest
{
    bool ShouldLogRequestDetails { get; }
    bool ShouldLogResponseDetails { get; }
    LogLevel LogLevel { get; }
}

public interface ISensitiveDataRequest
{
    void MaskSensitiveData();
}

public interface ISensitiveDataResponse
{
    // Marker interface for responses with sensitive data
}

public class RequestLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RequestLoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IRequestLogger _requestLogger;

    public RequestLoggingBehavior(
        ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger,
        IRequestLogger requestLogger)
    {
        _logger = logger;
        _requestLogger = requestLogger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestLog = new RequestLogEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            RequestType = typeof(TRequest).Name,
            Timestamp = DateTime.UtcNow,
            UserId = GetCurrentUserId(),
            IpAddress = GetCurrentIpAddress(),
            UserAgent = GetCurrentUserAgent()
        };

        try
        {
            // Log request start
            await _requestLogger.LogRequestStartAsync(requestLog);

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            // Log request completion
            requestLog.Duration = stopwatch.Elapsed;
            requestLog.Success = true;
            await _requestLogger.LogRequestEndAsync(requestLog);

            _logger.LogInformation("Request {RequestId} completed successfully in {Duration}ms",
                requestLog.RequestId, requestLog.Duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            requestLog.Success = false;
            requestLog.ErrorMessage = ex.Message;
            requestLog.ErrorType = ex.GetType().Name;

            await _requestLogger.LogRequestErrorAsync(requestLog, ex);

            _logger.LogError(ex, "Request {RequestId} failed: {ErrorMessage}",
                requestLog.RequestId, ex.Message);

            throw;
        }
    }

    private string? GetCurrentUserId()
    {
        // Implementation would get current user from HttpContext
        return null;
    }

    private string? GetCurrentIpAddress()
    {
        // Implementation would get IP from HttpContext
        return null;
    }

    private string? GetCurrentUserAgent()
    {
        // Implementation would get User-Agent from HttpContext
        return null;
    }
}

public interface IRequestLogger
{
    Task LogRequestStartAsync(RequestLogEntry entry);
    Task LogRequestEndAsync(RequestLogEntry entry);
    Task LogRequestErrorAsync(RequestLogEntry entry, Exception exception);
    Task<IEnumerable<RequestLogEntry>> GetRecentRequestsAsync(int count = 100);
    Task<RequestMetrics> GetRequestMetricsAsync(DateTime startDate, DateTime endDate);
}

public class RequestLogEntry
{
    public string RequestId { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class RequestMetrics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, int> RequestsByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<string> TopErrors { get; set; } = new();
}

public class DatabaseRequestLogger : IRequestLogger
{
    // Implementation would use Entity Framework or other data access
    // to store request logs in database

    public Task LogRequestStartAsync(RequestLogEntry entry)
    {
        // TODO: Implement database logging
        return Task.CompletedTask;
    }

    public Task LogRequestEndAsync(RequestLogEntry entry)
    {
        // TODO: Implement database logging
        return Task.CompletedTask;
    }

    public Task LogRequestErrorAsync(RequestLogEntry entry, Exception exception)
    {
        // TODO: Implement database logging
        return Task.CompletedTask;
    }

    public Task<IEnumerable<RequestLogEntry>> GetRecentRequestsAsync(int count = 100)
    {
        // TODO: Implement database query
        return Task.FromResult(Enumerable.Empty<RequestLogEntry>());
    }

    public Task<RequestMetrics> GetRequestMetricsAsync(DateTime startDate, DateTime endDate)
    {
        // TODO: Implement metrics calculation
        return Task.FromResult(new RequestMetrics());
    }
}

public static class LoggingExtensions
{
    public static ILoggingBuilder AddRequestLogging(this ILoggingBuilder builder)
    {
        return builder.AddFilter("CommunityCar.Application.Behaviors.LoggingBehavior", LogLevel.Information);
    }

    public static void LogBusinessEvent(this ILogger logger, string eventName, object data = null, string userId = null)
    {
        var logData = new
        {
            EventName = eventName,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Data = data
        };

        logger.LogInformation("Business Event: {EventName} - {@LogData}", eventName, logData);
    }

    public static void LogPerformanceMetric(this ILogger logger, string operation, TimeSpan duration, bool success = true)
    {
        logger.LogInformation("Performance: {Operation} completed in {Duration}ms (Success: {Success})",
            operation, duration.TotalMilliseconds, success);
    }

    public static void LogSecurityEvent(this ILogger logger, string eventType, string userId, string details = null)
    {
        logger.LogWarning("Security Event: {EventType} - User: {UserId} - Details: {Details}",
            eventType, userId, details ?? "N/A");
    }
}