using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeApiKeyAttribute : Attribute, IAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryName = "api_key";

    private readonly ILogger<AuthorizeApiKeyAttribute> _logger;
    private readonly IConfiguration _configuration;

    public AuthorizeApiKeyAttribute()
    {
        // Dependencies will be resolved at runtime
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Get services from request services
        var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<AuthorizeApiKeyAttribute>)) as ILogger<AuthorizeApiKeyAttribute>;
        var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;

        if (logger == null || configuration == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var apiKey = GetApiKeyFromRequest(context.HttpContext);

        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogWarning("API key missing in request from {RemoteIpAddress}",
                context.HttpContext.Connection.RemoteIpAddress);

            context.Result = new UnauthorizedObjectResult(new
            {
                error = "API key required",
                message = "Please provide a valid API key in the request header or query parameter"
            });
            return;
        }

        var validApiKeys = configuration.GetSection("ApiKeys:ValidKeys").Get<string[]>() ?? Array.Empty<string>();

        if (!validApiKeys.Contains(apiKey))
        {
            logger.LogWarning("Invalid API key attempt from {RemoteIpAddress}: {ApiKeyPrefix}...",
                context.HttpContext.Connection.RemoteIpAddress,
                apiKey.Length > 8 ? apiKey.Substring(0, 8) : apiKey);

            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Invalid API key",
                message = "The provided API key is not valid"
            });
            return;
        }

        // Store API key in HttpContext for later use
        context.HttpContext.Items["ApiKey"] = apiKey;

        logger.LogInformation("API key authentication successful for request from {RemoteIpAddress}",
            context.HttpContext.Connection.RemoteIpAddress);
    }

    private string? GetApiKeyFromRequest(HttpContext context)
    {
        // Try header first
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
        {
            return headerValue.ToString();
        }

        // Try query parameter
        if (context.Request.Query.TryGetValue(ApiKeyQueryName, out var queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }
}

public class ApiKeyRequirement
{
    public string[] ValidKeys { get; set; } = Array.Empty<string>();
    public bool RequireHttps { get; set; } = true;
    public string[] AllowedIpAddresses { get; set; } = Array.Empty<string>();
}

public static class ApiKeyExtensions
{
    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiKeyRequirement>(configuration.GetSection("ApiKeys"));
        return services;
    }

    public static string? GetApiKey(this HttpContext context)
    {
        return context.Items["ApiKey"] as string;
    }

    public static bool HasValidApiKey(this HttpContext context)
    {
        return !string.IsNullOrEmpty(context.GetApiKey());
    }
}