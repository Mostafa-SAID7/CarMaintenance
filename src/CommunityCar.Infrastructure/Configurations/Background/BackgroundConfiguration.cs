using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Background;
using CommunityCar.Infrastructure.Configurations.Background.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CommunityCar.Infrastructure.Configurations.Background;

/// <summary>
/// Configuration class for setting up background job processing.
/// </summary>
public static class BackgroundConfiguration
{
    /// <summary>
    /// Adds background job configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBackgroundConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register background job service
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // Configure background job settings with validation
        services.AddOptions<BackgroundJobSettings>()
            .Bind(configuration.GetSection("BackgroundJobs"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register job queues (in-memory for now, can be replaced with distributed cache)
        services.AddSingleton<IBackgroundJobQueue, InMemoryJobQueue>();

        // Register common job types
        services.AddTransient<EmailJob>();
        services.AddTransient<CleanupJob>();
        services.AddTransient<NotificationJob>();
        services.AddTransient<AnalyticsJob>();

        // Configure job processing options
        services.AddHostedService<BackgroundJobHostedService>();

        return services;
    }
}
