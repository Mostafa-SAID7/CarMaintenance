using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Infrastructure.Configurations.Caching;

/// <summary>
/// Configuration settings for caching behavior
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    [Range(1, 1440)] // 1 minute to 24 hours
    public int DefaultExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Long-term cache expiration in hours
    /// </summary>
    [Range(1, 168)] // 1 hour to 1 week
    public int LongTermExpirationHours { get; set; } = 24;

    /// <summary>
    /// Short-term cache expiration in seconds
    /// </summary>
    [Range(1, 3600)] // 1 second to 1 hour
    public int ShortTermExpirationSeconds { get; set; } = 60;

    /// <summary>
    /// Whether to enable compression for cached data
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Maximum cache size in MB
    /// </summary>
    [Range(1, 10240)] // 1MB to 10GB
    public int MaxCacheSizeMB { get; set; } = 1024;

    /// <summary>
    /// Whether to enable cache warmup on startup
    /// </summary>
    public bool EnableCacheWarmup { get; set; } = true;

    /// <summary>
    /// Interval for cache warmup in minutes
    /// </summary>
    [Range(1, 1440)] // 1 minute to 24 hours
    public int CacheWarmupIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// Keys to warmup during cache warmup process
    /// </summary>
    public string[] CacheWarmupKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to enable cache statistics collection
    /// </summary>
    public bool EnableCacheStatistics { get; set; } = true;

    /// <summary>
    /// How long to retain statistics in hours
    /// </summary>
    [Range(1, 168)] // 1 hour to 1 week
    public int StatisticsRetentionHours { get; set; } = 24;

    /// <summary>
    /// Validates the cache settings
    /// </summary>
    public void Validate()
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);

        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            throw new ValidationException($"Cache settings validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
        }
    }
}
