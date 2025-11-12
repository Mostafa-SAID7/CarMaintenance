using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Filters;

public class ApiValidationFilter : IActionFilter
{
    private readonly ILogger<ApiValidationFilter> _logger;

    public ApiValidationFilter(ILogger<ApiValidationFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            _logger.LogWarning("Model validation failed for {Action} in {Controller}",
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name);

            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Any() == true)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var validationError = new ValidationErrorResponse
            {
                Message = "One or more validation errors occurred.",
                Errors = errors,
                Timestamp = DateTime.UtcNow,
                Path = context.HttpContext.Request.Path,
                Method = context.HttpContext.Request.Method
            };

            context.Result = new BadRequestObjectResult(validationError);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}

public class ValidationErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string TraceId => System.Diagnostics.Activity.Current?.Id ?? string.Empty;
}