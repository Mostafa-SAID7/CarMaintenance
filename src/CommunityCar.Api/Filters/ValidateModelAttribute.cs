using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly ILogger<ValidateModelAttribute> _logger;

    public ValidateModelAttribute()
    {
        // Logger will be resolved at runtime
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<ValidateModelAttribute>)) as ILogger<ValidateModelAttribute>;

        if (!context.ModelState.IsValid)
        {
            logger?.LogWarning("Model validation failed for action {ActionName} in controller {ControllerName}",
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name);

            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(error => new
                {
                    Field = ms.Key,
                    Message = error.ErrorMessage,
                    Code = error.ErrorCode
                }))
                .ToList();

            var errorResponse = new
            {
                success = false,
                message = "Validation failed",
                errors = errors,
                timestamp = DateTime.UtcNow,
                requestId = context.HttpContext.TraceIdentifier
            };

            context.Result = new BadRequestObjectResult(errorResponse);
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ValidateApiModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, List<string>>();

            foreach (var modelState in context.ModelState)
            {
                if (modelState.Value?.Errors.Count > 0)
                {
                    errors[modelState.Key] = modelState.Value.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();
                }
            }

            var apiError = new ApiErrorResponse
            {
                StatusCode = 400,
                Message = "One or more validation errors occurred.",
                Errors = errors,
                Path = context.HttpContext.Request.Path,
                Method = context.HttpContext.Request.Method,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new BadRequestObjectResult(apiError);
        }
    }
}

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TraceId => System.Diagnostics.Activity.Current?.Id ?? string.Empty;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class DetailedValidationAttribute : ActionFilterAttribute
{
    public bool IncludeStackTrace { get; set; } = false;
    public bool IncludeInnerExceptions { get; set; } = false;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var validationDetails = new ValidationDetails
            {
                IsValid = false,
                Errors = new List<ValidationErrorDetail>(),
                AttemptedValues = new Dictionary<string, object?>(),
                Timestamp = DateTime.UtcNow,
                RequestPath = context.HttpContext.Request.Path,
                RequestMethod = context.HttpContext.Request.Method
            };

            foreach (var modelState in context.ModelState)
            {
                if (modelState.Value?.Errors.Count > 0)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        var errorDetail = new ValidationErrorDetail
                        {
                            Field = modelState.Key,
                            Message = error.ErrorMessage,
                            ErrorCode = error.ErrorCode,
                            ExceptionType = error.Exception?.GetType().Name
                        };

                        if (IncludeStackTrace && error.Exception != null)
                        {
                            errorDetail.StackTrace = error.Exception.StackTrace;
                        }

                        if (IncludeInnerExceptions && error.Exception?.InnerException != null)
                        {
                            errorDetail.InnerException = error.Exception.InnerException.Message;
                        }

                        validationDetails.Errors.Add(errorDetail);
                    }
                }

                // Try to get the attempted value
                if (context.ActionArguments.TryGetValue(modelState.Key, out var attemptedValue))
                {
                    validationDetails.AttemptedValues[modelState.Key] = attemptedValue;
                }
            }

            context.Result = new BadRequestObjectResult(validationDetails);
        }
    }
}

public class ValidationDetails
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDetail> Errors { get; set; } = new();
    public Dictionary<string, object?> AttemptedValues { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string RequestPath { get; set; } = string.Empty;
    public string RequestMethod { get; set; } = string.Empty;
    public string TraceId => System.Diagnostics.Activity.Current?.Id ?? string.Empty;
}

public class ValidationErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class CustomValidationAttribute : ActionFilterAttribute
{
    private readonly string _validationRule;

    public CustomValidationAttribute(string validationRule)
    {
        _validationRule = validationRule;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Custom validation logic based on the rule
        switch (_validationRule.ToLowerInvariant())
        {
            case "strict":
                ValidateStrict(context);
                break;
            case "lenient":
                ValidateLenient(context);
                break;
            case "business":
                ValidateBusinessRules(context);
                break;
            default:
                // Default validation
                if (!context.ModelState.IsValid)
                {
                    context.Result = new BadRequestObjectResult(new
                    {
                        error = "Validation failed",
                        details = context.ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }
                break;
        }
    }

    private void ValidateStrict(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Any() == true)
                .SelectMany(ms => ms.Value!.Errors.Select(e => new
                {
                    Field = ms.Key,
                    Message = e.ErrorMessage,
                    Severity = "Error"
                }));

            context.Result = new BadRequestObjectResult(new
            {
                validationMode = "strict",
                errors = errors,
                message = "All validation errors must be resolved"
            });
        }
    }

    private void ValidateLenient(ActionExecutingContext context)
    {
        // Allow some validation errors but log warnings
        var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<CustomValidationAttribute>)) as ILogger<CustomValidationAttribute>;

        if (!context.ModelState.IsValid)
        {
            var warnings = context.ModelState
                .Where(ms => ms.Value?.Errors.Any() == true)
                .SelectMany(ms => ms.Value!.Errors.Select(e => new
                {
                    Field = ms.Key,
                    Message = e.ErrorMessage,
                    Severity = "Warning"
                }));

            logger?.LogWarning("Validation warnings for request: {@Warnings}", warnings);

            // Don't fail the request, just log the warnings
        }
    }

    private void ValidateBusinessRules(ActionExecutingContext context)
    {
        // Custom business rule validation
        // This would contain specific business logic validation

        if (!context.ModelState.IsValid)
        {
            var businessErrors = context.ModelState
                .Where(ms => ms.Value?.Errors.Any() == true)
                .SelectMany(ms => ms.Value!.Errors.Select(e => new
                {
                    Field = ms.Key,
                    Message = e.ErrorMessage,
                    RuleType = "BusinessRule"
                }));

            context.Result = new BadRequestObjectResult(new
            {
                validationMode = "business",
                errors = businessErrors,
                message = "Business rule validation failed"
            });
        }
    }
}

public static class ValidationExtensions
{
    public static IServiceCollection AddCustomValidation(this IServiceCollection services)
    {
        // Register any validation services here
        return services;
    }

    public static IApplicationBuilder UseCustomValidation(this IApplicationBuilder app)
    {
        // Add any validation middleware here
        return app;
    }
}