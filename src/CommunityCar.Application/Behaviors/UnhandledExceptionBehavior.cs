using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CommunityCar.Application.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly INotificationService _notificationService;

    public UnhandledExceptionBehavior(
        ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger,
        IExceptionHandler exceptionHandler,
        INotificationService notificationService)
    {
        _logger = logger;
        _exceptionHandler = exceptionHandler;
        _notificationService = notificationService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            var userId = GetCurrentUserId();

            // Log the exception with structured data
            _logger.LogError(ex, "Unhandled exception in {RequestName} for user {UserId}: {ExceptionMessage}",
                requestName, userId ?? "anonymous", ex.Message);

            // Create exception context
            var context = new ExceptionContext
            {
                RequestType = requestName,
                UserId = userId,
                Exception = ex,
                Timestamp = DateTime.UtcNow,
                RequestData = GetRequestData(request),
                AdditionalData = GetAdditionalContext()
            };

            // Handle the exception
            var handledException = await _exceptionHandler.HandleAsync(context);

            // Send notifications for critical errors
            if (IsCriticalException(ex))
            {
                await SendCriticalErrorNotificationAsync(context);
            }

            // Log the handling result
            _logger.LogInformation("Exception handled for {RequestName}: {HandlingResult}",
                requestName, handledException?.GetType().Name ?? "No specific handling");

            // Re-throw the original or handled exception
            throw handledException ?? ex;
        }
    }

    private string? GetCurrentUserId()
    {
        // Implementation would get current user from HttpContext
        return null;
    }

    private object? GetRequestData(TRequest request)
    {
        try
        {
            // Return sanitized request data for logging
            if (request is ISensitiveDataRequest)
            {
                return "[Request contains sensitive data]";
            }

            return request;
        }
        catch
        {
            return "[Unable to serialize request]";
        }
    }

    private Dictionary<string, object>? GetAdditionalContext()
    {
        return new Dictionary<string, object>
        {
            ["MachineName"] = Environment.MachineName,
            ["ProcessId"] = Process.GetCurrentProcess().Id,
            ["ThreadId"] = Environment.CurrentManagedThreadId,
            ["Timestamp"] = DateTime.UtcNow
        };
    }

    private bool IsCriticalException(Exception ex)
    {
        return ex is OutOfMemoryException ||
               ex is StackOverflowException ||
               ex is AccessViolationException ||
               ex.GetType().Name.Contains("Database") ||
               ex.GetType().Name.Contains("Timeout");
    }

    private async Task SendCriticalErrorNotificationAsync(ExceptionContext context)
    {
        try
        {
            var notification = new CriticalErrorNotification
            {
                RequestType = context.RequestType,
                UserId = context.UserId,
                ExceptionType = context.Exception.GetType().Name,
                ExceptionMessage = context.Exception.Message,
                Timestamp = context.Timestamp,
                MachineName = Environment.MachineName
            };

            await _notificationService.SendCriticalErrorNotificationAsync(notification);
        }
        catch (Exception notificationEx)
        {
            _logger.LogError(notificationEx, "Failed to send critical error notification");
        }
    }
}

public interface IExceptionHandler
{
    Task<Exception?> HandleAsync(ExceptionContext context);
}

public class ExceptionContext
{
    public string RequestType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Exception Exception { get; set; } = new Exception();
    public DateTime Timestamp { get; set; }
    public object? RequestData { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class DefaultExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DefaultExceptionHandler> _logger;

    public DefaultExceptionHandler(ILogger<DefaultExceptionHandler> logger)
    {
        _logger = logger;
    }

    public Task<Exception?> HandleAsync(ExceptionContext context)
    {
        // Default behavior: don't modify the exception
        // Subclasses can override this to provide custom handling

        _logger.LogInformation("Default exception handling for {RequestType}: {ExceptionType}",
            context.RequestType, context.Exception.GetType().Name);

        return Task.FromResult<Exception?>(null);
    }
}

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public Task<Exception?> HandleAsync(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationEx)
        {
            _logger.LogWarning("Validation exception in {RequestType}: {Message}",
                context.RequestType, validationEx.Message);

            // Return a more user-friendly exception
            return Task.FromResult<Exception?>(
                new BadRequestException("Validation failed", validationEx));
        }

        return Task.FromResult<Exception?>(null);
    }
}

public class DatabaseExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DatabaseExceptionHandler> _logger;

    public DatabaseExceptionHandler(ILogger<DatabaseExceptionHandler> logger)
    {
        _logger = logger;
    }

    public Task<Exception?> HandleAsync(ExceptionContext context)
    {
        var ex = context.Exception;

        if (ex.GetType().Name.Contains("DbException") ||
            ex.GetType().Name.Contains("SqlException") ||
            ex.Message.Contains("database", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Database exception in {RequestType}", context.RequestType);

            // Return a generic database error without exposing internal details
            return Task.FromResult<Exception?>(
                new InternalServerException("A database error occurred. Please try again later."));
        }

        return Task.FromResult<Exception?>(null);
    }
}

public class CompositeExceptionHandler : IExceptionHandler
{
    private readonly IEnumerable<IExceptionHandler> _handlers;
    private readonly ILogger<CompositeExceptionHandler> _logger;

    public CompositeExceptionHandler(
        IEnumerable<IExceptionHandler> handlers,
        ILogger<CompositeExceptionHandler> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async Task<Exception?> HandleAsync(ExceptionContext context)
    {
        foreach (var handler in _handlers)
        {
            try
            {
                var result = await handler.HandleAsync(context);
                if (result != null)
                {
                    return result;
                }
            }
            catch (Exception handlerEx)
            {
                _logger.LogError(handlerEx, "Exception handler {HandlerType} failed",
                    handler.GetType().Name);
            }
        }

        return null;
    }
}

public class ExceptionLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionLoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IExceptionLogger _exceptionLogger;

    public ExceptionLoggingBehavior(
        ILogger<ExceptionLoggingBehavior<TRequest, TResponse>> logger,
        IExceptionLogger exceptionLogger)
    {
        _logger = logger;
        _exceptionLogger = exceptionLogger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            // Log to structured logger
            await _exceptionLogger.LogExceptionAsync(new ExceptionLogEntry
            {
                RequestType = requestName,
                Exception = ex,
                Timestamp = DateTime.UtcNow,
                UserId = GetCurrentUserId(),
                RequestData = request,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.Message
            });

            throw;
        }
    }

    private string? GetCurrentUserId()
    {
        // Implementation would get current user from HttpContext
        return null;
    }
}

public interface IExceptionLogger
{
    Task LogExceptionAsync(ExceptionLogEntry entry);
    Task<IEnumerable<ExceptionLogEntry>> GetRecentExceptionsAsync(int count = 100);
    Task<ExceptionStatistics> GetExceptionStatisticsAsync(DateTime startDate, DateTime endDate);
}

public class ExceptionLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RequestType { get; set; } = string.Empty;
    public Exception Exception { get; set; } = new Exception();
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public object? RequestData { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public string? MachineName { get; set; } = Environment.MachineName;
    public string? Environment { get; set; } = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
}

public class ExceptionStatistics
{
    public int TotalExceptions { get; set; }
    public Dictionary<string, int> ExceptionsByType { get; set; } = new();
    public Dictionary<string, int> ExceptionsByRequestType { get; set; } = new();
    public List<string> MostCommonExceptions { get; set; } = new();
    public double AverageExceptionsPerDay { get; set; }
}

public class DatabaseExceptionLogger : IExceptionLogger
{
    // Implementation would use Entity Framework to store exceptions in database

    public Task LogExceptionAsync(ExceptionLogEntry entry)
    {
        // TODO: Implement database logging
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ExceptionLogEntry>> GetRecentExceptionsAsync(int count = 100)
    {
        // TODO: Implement database query
        return Task.FromResult(Enumerable.Empty<ExceptionLogEntry>());
    }

    public Task<ExceptionStatistics> GetExceptionStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        // TODO: Implement statistics calculation
        return Task.FromResult(new ExceptionStatistics());
    }
}

public interface INotificationService
{
    Task SendCriticalErrorNotificationAsync(CriticalErrorNotification notification);
}

public class CriticalErrorNotification
{
    public string RequestType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string ExceptionType { get; set; } = string.Empty;
    public string ExceptionMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string MachineName { get; set; } = string.Empty;
}

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionHandler, CompositeExceptionHandler>();
        services.AddSingleton<IExceptionLogger, DatabaseExceptionLogger>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Register individual handlers
        services.AddSingleton<IExceptionHandler, ValidationExceptionHandler>();
        services.AddSingleton<IExceptionHandler, DatabaseExceptionHandler>();

        services.AddMediatR(cfg =>
        {
            cfg.AddBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddBehavior(typeof(ExceptionLoggingBehavior<,>));
        });

        return services;
    }
}

// Custom exception types
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
    public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
}

public class InternalServerException : Exception
{
    public InternalServerException(string message) : base(message) { }
    public InternalServerException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendCriticalErrorNotificationAsync(CriticalErrorNotification notification)
    {
        _logger.LogCritical("Critical error notification: {@Notification}", notification);
        // TODO: Implement actual notification sending (email, Slack, etc.)
        return Task.CompletedTask;
    }
}
