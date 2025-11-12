using CommunityCar.Api.Models;
using CommunityCar.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CommunityCar.Api.Handlers;

public class ExceptionHandlers
{
    private readonly ILogger<ExceptionHandlers> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlers(ILogger<ExceptionHandlers> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Handles domain-specific exceptions
    /// </summary>
    public async Task HandleDomainException(HttpContext context, DomainException exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogWarning(exception, "Domain exception occurred. TraceId: {TraceId}, RequestId: {RequestId}",
            traceId, requestId);

        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Request-ID"] = requestId;

        var statusCode = exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            ValidationException => HttpStatusCode.BadRequest,
            BusinessRuleViolationException => HttpStatusCode.UnprocessableEntity,
            ConcurrencyException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var errorResponse = ErrorResponse.Create(exception.Message, null, traceId);
        await WriteJsonResponse(context, errorResponse);
    }

    /// <summary>
    /// Handles application-specific exceptions
    /// </summary>
    public async Task HandleApplicationException(HttpContext context, ApplicationException exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogError(exception, "Application exception occurred. TraceId: {TraceId}, RequestId: {RequestId}, ErrorCode: {ErrorCode}",
            traceId, requestId, exception.ErrorCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.Headers["X-Request-ID"] = requestId;

        var message = _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred";

        var errorResponse = ErrorResponse.Create(message, null, traceId, exception.ErrorCode);
        await WriteJsonResponse(context, errorResponse);
    }

    /// <summary>
    /// Handles validation exceptions from FluentValidation
    /// </summary>
    public async Task HandleValidationException(HttpContext context, FluentValidation.ValidationException exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogWarning(exception, "Validation exception occurred. TraceId: {TraceId}, RequestId: {RequestId}",
            traceId, requestId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.Headers["X-Request-ID"] = requestId;

        var errors = exception.Errors.Select(e => e.ErrorMessage).ToList();
        var errorResponse = ErrorResponse.Create("Validation failed", errors, traceId);
        await WriteJsonResponse(context, errorResponse);
    }

    /// <summary>
    /// Handles general exceptions
    /// </summary>
    public async Task HandleGeneralException(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}, RequestId: {RequestId}, Path: {Path}, Method: {Method}",
            traceId, requestId, context.Request.Path, context.Request.Method);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.Headers["X-Request-ID"] = requestId;

        var message = _environment.IsDevelopment()
            ? $"An unexpected error occurred: {exception.Message}"
            : "An unexpected error occurred";

        var errorResponse = ErrorResponse.Create(message, null, traceId);
        await WriteJsonResponse(context, errorResponse);
    }

    /// <summary>
    /// Handles timeout exceptions
    /// </summary>
    public async Task HandleTimeoutException(HttpContext context, TimeoutException exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogWarning(exception, "Request timeout occurred. TraceId: {TraceId}, RequestId: {RequestId}",
            traceId, requestId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
        context.Response.Headers["X-Request-ID"] = requestId;

        var errorResponse = ErrorResponse.Create("Request timed out", null, traceId);
        await WriteJsonResponse(context, errorResponse);
    }

    /// <summary>
    /// Handles operation canceled exceptions
    /// </summary>
    public async Task HandleOperationCanceledException(HttpContext context, OperationCanceledException exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogInformation("Request was cancelled. TraceId: {TraceId}, RequestId: {RequestId}",
            traceId, requestId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
        context.Response.Headers["X-Request-ID"] = requestId;

        var errorResponse = ErrorResponse.Create("Request was cancelled", null, traceId);
        await WriteJsonResponse(context, errorResponse);
    }

    private async Task WriteJsonResponse(HttpContext context, ErrorResponse errorResponse)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
    }

    /// <summary>
    /// Extension method to use exception handlers in middleware
    /// </summary>
    public static async Task HandleExceptionWithHandlers(HttpContext context, Exception exception, IServiceProvider services)
    {
        var handlers = services.GetRequiredService<ExceptionHandlers>();

        switch (exception)
        {
            case DomainException domainEx:
                await handlers.HandleDomainException(context, domainEx);
                break;
            case ApplicationException appEx:
                await handlers.HandleApplicationException(context, appEx);
                break;
            case FluentValidation.ValidationException validationEx:
                await handlers.HandleValidationException(context, validationEx);
                break;
            case TimeoutException timeoutEx:
                await handlers.HandleTimeoutException(context, timeoutEx);
                break;
            case OperationCanceledException canceledEx:
                await handlers.HandleOperationCanceledException(context, canceledEx);
                break;
            default:
                await handlers.HandleGeneralException(context, exception);
                break;
        }
    }
}
