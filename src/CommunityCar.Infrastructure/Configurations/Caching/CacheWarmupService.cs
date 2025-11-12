using CommunityCar.Application.Interfaces;
using CommunityCar.Application.Interfaces.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommunityCar.Infrastructure.Configurations.Caching;

/// <summary>
/// Background service for warming up cache with frequently accessed data
/// </summary>
public class CacheWarmupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CacheSettings _settings;
    private readonly ILogger<CacheWarmupService> _logger;

    // Constants for cache warmup keys
    private const string UserProfileKey = "user_profile";
    private const string SystemSettingsKey = "system_settings";
    private const string LookupDataKey = "lookup_data";

    public CacheWarmupService(
        IServiceProvider serviceProvider,
        IOptions<CacheSettings> settings,
        ILogger<CacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableCacheWarmup)
        {
            _logger.LogInformation("Cache warmup is disabled");
            return;
        }

        _logger.LogInformation("Cache warmup service started with interval: {Interval} minutes", _settings.CacheWarmupIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await WarmupCacheAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache warmup cycle");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_settings.CacheWarmupIntervalMinutes), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected when stopping
                break;
            }
        }

        _logger.LogInformation("Cache warmup service stopped");
    }

    private async Task WarmupCacheAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var cacheService = serviceProvider.GetRequiredService<ICacheService>();

            _logger.LogInformation("Starting cache warmup for {Count} keys", _settings.CacheWarmupKeys.Length);

            var warmupTasks = new List<Task>();

            foreach (var key in _settings.CacheWarmupKeys)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var task = key switch
                    {
                        UserProfileKey => WarmupUserProfilesAsync(cacheService, serviceProvider, cancellationToken),
                        SystemSettingsKey => WarmupSystemSettingsAsync(cacheService, serviceProvider, cancellationToken),
                        LookupDataKey => WarmupLookupDataAsync(cacheService, serviceProvider, cancellationToken),
                        _ => Task.Run(() => _logger.LogWarning("Unknown cache warmup key: {Key}", key), cancellationToken)
                    };

                    warmupTasks.Add(task);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error queuing warmup for cache key: {Key}", key);
                }
            }

            await Task.WhenAll(warmupTasks);

            _logger.LogInformation("Cache warmup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warmup process");
        }
    }

    private async Task WarmupUserProfilesAsync(ICacheService cacheService, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Warming up user profiles cache");

            // Get profile service to fetch frequently accessed profiles
            var profileService = serviceProvider.GetService<IProfileService>();
            if (profileService == null)
            {
                _logger.LogWarning("IProfileService not available for cache warmup");
                return;
            }

            // Example: Warm up profiles for active users (this would need actual implementation)
            // For now, this is a placeholder - in real implementation, you'd fetch active user IDs
            var activeUserIds = await GetActiveUserIdsAsync(serviceProvider, cancellationToken);

            foreach (var userId in activeUserIds)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var cacheKey = $"profile:{userId}";
                    await cacheService.GetOrSetAsync(
                        cacheKey,
                        async () => await profileService.GetProfileAsync(userId),
                        TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error warming up profile for user {UserId}", userId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user profiles cache warmup");
        }
    }

    private async Task WarmupSystemSettingsAsync(ICacheService cacheService, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Warming up system settings cache");

            // Example: Cache common system settings
            var settingsKeys = new[] { "app_config", "feature_flags", "maintenance_mode" };

            foreach (var key in settingsKeys)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var cacheKey = $"system:{key}";
                    // In real implementation, fetch from configuration or database
                    await cacheService.GetOrSetAsync(
                        cacheKey,
                        async () => await GetSystemSettingAsync(key, serviceProvider),
                        TimeSpan.FromHours(_settings.LongTermExpirationHours));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error warming up system setting: {Key}", key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system settings cache warmup");
        }
    }

    private async Task WarmupLookupDataAsync(ICacheService cacheService, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Warming up lookup data cache");

            // Example: Cache reference data like categories, tags, etc.
            var lookupKeys = new[] { "categories", "tags", "countries", "car_types" };

            foreach (var key in lookupKeys)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var cacheKey = $"lookup:{key}";
                    // In real implementation, fetch from repository
                    await cacheService.GetOrSetAsync(
                        cacheKey,
                        async () => await GetLookupDataAsync(key, serviceProvider),
                        TimeSpan.FromHours(_settings.LongTermExpirationHours));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error warming up lookup data: {Key}", key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during lookup data cache warmup");
        }
    }

    // Helper methods for fetching data - these would be implemented based on actual services
    private async Task<IEnumerable<string>> GetActiveUserIdsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query database for recently active users
        return Enumerable.Empty<string>();
    }

    private async Task<object?> GetSystemSettingAsync(string key, IServiceProvider serviceProvider)
    {
        // Placeholder - in real implementation, fetch from configuration service
        return null;
    }

    private async Task<object?> GetLookupDataAsync(string key, IServiceProvider serviceProvider)
    {
        // Placeholder - in real implementation, fetch from repository
        return null;
    }
}
