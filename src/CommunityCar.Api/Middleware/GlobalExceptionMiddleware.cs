using CommunityCar.Api.Handlers;
using CommunityCar.Api.Models;
using CommunityCar.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CommunityCar.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var exceptionHandlers = context.RequestServices.GetRequiredService<ExceptionHandlers>();
            await ExceptionHandlers.HandleExceptionWithHandlers(context, ex, context.RequestServices);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? traceId;

        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}, RequestId: {RequestId}, Path: {Path}, Method: {Method}",
            traceId, requestId, context.Request.Path, context.Request.Method);

        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Request-ID"] = requestId;

        ErrorResponse errorResponse;

        switch (exception)
        {
            case BadRequestException badRequestEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.Create(badRequestEx.Message, null, traceId);
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = ErrorResponse.Create(notFoundEx.Message, null, traceId);
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = ErrorResponse.Create(unauthorizedEx.Message, null, traceId);
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.Create(validationEx.Message, validationEx.Errors, traceId);
                break;

            case FluentValidation.ValidationException fluentValidationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = fluentValidationEx.Errors.Select(e => e.ErrorMessage);
                errorResponse = ErrorResponse.Create("Validation failed", errors, traceId);
                break;

            case OperationCanceledException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse = ErrorResponse.Create("Request was cancelled", null, traceId);
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse = ErrorResponse.Create("Request timed out", null, traceId);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var message = _environment.IsDevelopment()
                    ? $"An unexpected error occurred: {exception.Message}"
                    : "An unexpected error occurred";
                errorResponse = ErrorResponse.Create(message, null, traceId);
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
    }
}
