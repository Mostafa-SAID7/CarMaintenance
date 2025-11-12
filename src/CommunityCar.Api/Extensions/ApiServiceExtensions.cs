using CommunityCar.Api.Filters;
using CommunityCar.Api.Middleware;
using CommunityCar.Api.Resources;
using CommunityCar.Api.Swagger;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;

namespace CommunityCar.Api.Extensions;

public static class ApiServiceExtensions
{
    /// <summary>
    /// Adds API-specific services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers with custom options
        services.AddControllers(options =>
        {
            // Add global filters
            options.Filters.Add<AuthValidationFilter>();
            options.Filters.Add<AuthLoggingFilter>();

            // Configure model binding
            options.MaxModelBindingCollectionSize = 100;
            options.MaxModelValidationErrors = 50;
        })
        .AddJsonOptions(options =>
        {
            // Configure JSON serialization
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = false;
            options.JsonSerializerOptions.AllowTrailingCommas = true;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        // Add API versioning
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"),
                new QueryStringApiVersionReader("api-version")
            );
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Add Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Configure Swagger
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "CommunityCar API",
                Version = "v1",
                Description = "CommunityCar API for managing car community features",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "CommunityCar Support",
                    Email = "support@communitycar.com"
                }
            });

            // Add JWT Bearer token support
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Enable XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add operation filters
            options.OperationFilter<SwaggerDefaultValues>();
            options.OperationFilter<ApiKeyOperationFilter>();

            // Group endpoints by controller
            options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            options.DocInclusionPredicate((name, api) => true);
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });

            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                    new[] { "http://localhost:3000", "https://localhost:3000" };

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Add response compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = new[] { "text/plain", "text/css", "application/javascript", "text/html", "application/xml", "text/xml", "application/json", "text/json" };
        });

        // Add rate limiting (if using AspNetCoreRateLimit)
        // services.AddMemoryCache();
        // services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        // services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        // services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        // services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        // services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        // Add health checks
        services.AddHealthChecks();

        // Add HTTP client factory
        services.AddHttpClient();

        // Add localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("ar")
            };

            options.DefaultRequestCulture = new RequestCulture("en", "en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            // Configure culture providers (order matters)
            options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
            options.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());
            options.RequestCultureProviders.Insert(2, new AcceptLanguageHeaderRequestCultureProvider());
        });

        // Add SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = configuration.GetValue<bool>("SignalR:EnableDetailedErrors", false);
            options.MaximumReceiveMessageSize = 102400; // 100KB
        });

        // Add background services
        // services.AddHostedService<EmailBackgroundService>();
        // services.AddHostedService<AuditCleanupService>();

        return services;
    }

    /// <summary>
    /// Adds custom API behaviors and configurations
    /// </summary>
    public static IServiceCollection AddCustomApiBehaviors(this IServiceCollection services)
    {
        // Configure API behavior options
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true; // We'll handle validation manually
            options.SuppressMapClientErrors = true; // We'll handle client errors manually
        });

        // Add custom model validation
        services.AddScoped<ModelValidationFilter>();

        // Register exception handlers
        services.AddScoped<global::CommunityCar.Api.Handlers.ExceptionHandlers>();

        // Register cache service
        services.AddScoped<global::CommunityCar.Application.Interfaces.ICacheService,
            global::CommunityCar.Infrastructure.Services.CacheService>();

        // Register cached profile service (decorates the regular profile service)
        services.AddScoped<global::CommunityCar.Application.Interfaces.Auth.ICachedProfileService,
            global::CommunityCar.Infrastructure.Services.CachedProfileService>();

        return services;
    }
}

// Custom model validation filter
public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                message = "Validation failed",
                errors = errors,
                traceId = context.HttpContext.TraceIdentifier
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
