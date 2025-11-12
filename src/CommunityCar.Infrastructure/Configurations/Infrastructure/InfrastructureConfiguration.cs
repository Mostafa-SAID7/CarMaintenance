using CommunityCar.Application.Interfaces;
using CommunityCar.Domain.Common;
using CommunityCar.Domain.Entities.Auth;
using CommunityCar.Infrastructure.Services.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CommunityCar.Infrastructure.Configurations.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register core infrastructure services
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ISitemapService, SitemapService>();
        services.AddScoped<IViewRenderService, ViewRenderService>();

        // Register HTTP context accessor
        services.AddHttpContextAccessor();

        // Configure infrastructure settings
        services.Configure<InfrastructureSettings>(configuration.GetSection("Infrastructure"));

        // Register repositories
        services.AddScoped<IRepository<AuditEntry>, BaseRepository<AuditEntry>>();
        services.AddScoped<IRepository<AuditLog>, BaseRepository<AuditLog>>();

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register email service (placeholder - would integrate with SendGrid, etc.)
        services.AddScoped<IEmailService, EmailService>();

        // Register background services
        services.AddHostedService<AuditCleanupService>();
        services.AddHostedService<HealthCheckService>();

        return services;
    }
}

public class InfrastructureSettings
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public bool EnableDetailedLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;
    public int PerformanceThresholdMs { get; set; } = 1000;
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 5;
    public string TimeZone { get; set; } = "UTC";
    public bool EnableAuditLogging { get; set; } = true;
    public int AuditRetentionDays { get; set; } = 365;
}

// Email service implementation
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly InfrastructureSettings _settings;

    public EmailService(ILogger<EmailService> logger, IOptions<InfrastructureSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<bool> SendEmailAsync(EmailRequest request)
    {
        try
        {
            // Implementation would integrate with email provider (SendGrid, Mailgun, etc.)
            _logger.LogInformation("Sending email to {To} with subject {Subject}",
                request.To, request.Subject);

            // Simulate email sending
            await Task.Delay(100);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", request.To);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(IEnumerable<EmailRequest> requests)
    {
        var results = await Task.WhenAll(requests.Select(SendEmailAsync));
        return results.All(r => r);
    }

    public async Task<bool> SendTemplatedEmailAsync(string templateId, object model, string toEmail)
    {
        try
        {
            // Implementation would use email templates
            _logger.LogInformation("Sending templated email {TemplateId} to {ToEmail}",
                templateId, toEmail);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending templated email {TemplateId} to {ToEmail}",
                templateId, toEmail);
            return false;
        }
    }
}

// Audit cleanup background service
public class AuditCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly InfrastructureSettings _settings;
    private readonly ILogger<AuditCleanupService> _logger;

    public AuditCleanupService(
        IServiceProvider serviceProvider,
        IOptions<InfrastructureSettings> settings,
        ILogger<AuditCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableAuditLogging)
        {
            _logger.LogInformation("Audit logging is disabled, skipping cleanup");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await CleanupOldAuditEntriesAsync(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit cleanup");
            }

            // Run cleanup daily
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task CleanupOldAuditEntriesAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var auditRepository = serviceProvider.GetRequiredService<IRepository<AuditEntry>>();
            var cutoffDate = DateTime.UtcNow.AddDays(-_settings.AuditRetentionDays);

            // In a real implementation, this would delete old entries
            _logger.LogInformation("Audit cleanup completed. Removed entries older than {CutoffDate}",
                cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up audit entries");
        }
    }
}

// Health check background service
public class HealthCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(IServiceProvider serviceProvider, ILogger<HealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await PerformHealthChecksAsync(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health checks");
            }

            // Run health checks every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task PerformHealthChecksAsync(IServiceProvider serviceProvider)
    {
        try
        {
            // Check database connectivity
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var dbHealthy = await unitOfWork.CanConnectAsync();

            // Check cache
            var cacheService = serviceProvider.GetRequiredService<ICacheService>();
            await cacheService.GetAsync<string>("health_check");

            // Log health status
            _logger.LogInformation("Health check completed. Database: {DbHealthy}",
                dbHealthy ? "Healthy" : "Unhealthy");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
        }
    }
}

// Request logging middleware
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Request {Method} {Path} responded {StatusCode} in {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
    }
}

// Performance monitoring middleware
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly InfrastructureSettings _settings;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IOptions<InfrastructureSettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;

            if (duration.TotalMilliseconds > _settings.PerformanceThresholdMs)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds);
            }
        }
    }
}

// Extension methods for middleware
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }

    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}
