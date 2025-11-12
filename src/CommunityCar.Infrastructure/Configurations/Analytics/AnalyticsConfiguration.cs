using CommunityCar.Application.Interfaces;
using CommunityCar.Domain.Entities;
using CommunityCar.Domain.Utilities;
using CommunityCar.Infrastructure.Services.Analytics;
using CommunityCar.Infrastructure.Services.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CommunityCar.Infrastructure.Configurations.Analytics;

/// <summary>
/// Configuration for analytics services
/// </summary>
public static class AnalyticsConfiguration
{
    /// <summary>
    /// Configuration section name for analytics settings
    /// </summary>
    private const string AnalyticsSection = SD.AnalyticsSection;

    /// <summary>
    /// Adds analytics configuration to the service collection
    /// </summary>
    public static IServiceCollection AddAnalyticsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        #region Core Analytics Services
        // Register core analytics services for event tracking and visitor monitoring
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IVisitorTrackingService, VisitorTrackingService>();
        #endregion

        #region Supporting Services
        // Register supporting services for geo-location and device detection
        services.AddScoped<IGeoLocationService, GeoLocationService>();
        services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
        #endregion

        #region Configuration and Validation
        // Configure analytics settings with validation to ensure proper configuration
        services.Configure<AnalyticsSettings>(configuration.GetSection(AnalyticsSection));
        services.AddSingleton<IValidateOptions<AnalyticsSettings>, AnalyticsSettingsValidator>();
        #endregion

        #region Repositories
        // Register repositories for analytics data persistence
        services.AddScoped<IRepository<AnalyticsEvent>, BaseRepository<AnalyticsEvent>>();
        services.AddScoped<IRepository<UserAnalytics>, BaseRepository<UserAnalytics>>();
        services.AddScoped<IRepository<ContentAnalytics>, BaseRepository<ContentAnalytics>>();
        #endregion

        return services;
    }
}
