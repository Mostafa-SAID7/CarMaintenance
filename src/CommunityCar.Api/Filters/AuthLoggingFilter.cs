using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Filters;

public class AuthLoggingFilter : IActionFilter
{
    private readonly ILogger<AuthLoggingFilter> _logger;

    public AuthLoggingFilter(ILogger<AuthLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var action = context.ActionDescriptor.DisplayName;
        var controller = context.Controller.GetType().Name;

        _logger.LogInformation("Auth action executing: {Controller}.{Action} by user {UserId}",
            controller, action, userId);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var statusCode = context.HttpContext.Response.StatusCode;
        var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var action = context.ActionDescriptor.DisplayName;
        var controller = context.Controller.GetType().Name;

        if (statusCode >= 400)
        {
            _logger.LogWarning("Auth action failed: {Controller}.{Action} by user {UserId} with status {StatusCode}",
                controller, action, userId, statusCode);
        }
        else
        {
            _logger.LogInformation("Auth action completed: {Controller}.{Action} by user {UserId} with status {StatusCode}",
                controller, action, userId, statusCode);
        }
    }
}
