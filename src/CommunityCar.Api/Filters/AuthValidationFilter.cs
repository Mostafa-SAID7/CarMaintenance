using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Filters;

public class AuthValidationFilter : IActionFilter
{
    private readonly ILogger<AuthValidationFilter> _logger;

    public AuthValidationFilter(ILogger<AuthValidationFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            _logger.LogWarning("Model validation failed for {Controller}.{Action}",
                context.Controller.GetType().Name,
                context.ActionDescriptor.DisplayName);

            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                message = "Validation failed",
                errors = errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
