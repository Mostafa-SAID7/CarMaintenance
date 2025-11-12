using CommunityCar.Application.Interfaces;
using CommunityCar.Application.Interfaces.Auth;
using CommunityCar.Infrastructure.Services.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CommunityCar.Infrastructure.Configurations.Caching;

/// <summary>
/// Configuration for caching services
/// </summary>
public static class CachingConfiguration
{
    /// <summary>
    /// Adds caching configuration to the service collection
    /// </summary>
    public static IServiceCollection AddCachingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure memory cache with settings
        services.AddMemoryCache(options =>
        {
            // Size limit will be configured via CacheSettings
            options.CompactionPercentage = 0.1; // Compact 10% when limit reached
        });

        // Configure distributed cache (Redis/SQL Server)
        services.AddDistributedMemoryCache(); // Fallback to memory

        // Try to add Redis if configured
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "CommunityCar:";
            });
        }

        // Configure caching settings with validation
        services.Configure<CacheSettings>(configuration.GetSection("Caching"));
        services.AddSingleton<IValidateOptions<CacheSettings>, CacheSettingsValidator>();

        // Register core cache services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICachedProfileService, CachedProfileService>();

        // Register cache management services
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddSingleton<ICacheStatisticsService, CacheStatisticsService>();

        // Register cache warming service conditionally
        services.AddOptions<CacheSettings>()
            .Configure(options => configuration.GetSection("Caching").Bind(options))
            .ValidateOnStart();

        services.AddHostedService<CacheWarmupService>();

        return services;
    }
}

/// <summary>
/// Validator for CacheSettings
/// </summary>
public class CacheSettingsValidator : IValidateOptions<CacheSettings>
{
    public ValidateOptionsResult Validate(string? name, CacheSettings options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail($"Cache settings validation failed: {ex.Message}");
        }
    }
}
