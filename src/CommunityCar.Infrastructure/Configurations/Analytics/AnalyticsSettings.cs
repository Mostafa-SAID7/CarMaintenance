using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Infrastructure.Configurations.Analytics;

/// <summary>
/// Configuration settings for analytics behavior
/// </summary>
public sealed class AnalyticsSettings
{
    /// <summary>
    /// Whether to enable real-time tracking of analytics events
    /// </summary>
    public bool EnableRealTimeTracking { get; set; } = true;

    /// <summary>
    /// Cache expiration time for analytics data in minutes
    /// </summary>
    [DisplayName("Cache Expiration Minutes")]
    [Range(1, 1440)] // 1 minute to 24 hours
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Maximum number of events to process in a single batch
    /// </summary>
    [Range(1, 10000)] // 1 to 10,000 events
    public int MaxEventsPerBatch { get; set; } = 1000;

    /// <summary>
    /// Whether to enable geo-location tracking
    /// </summary>
    public bool EnableGeoLocation { get; set; } = true;

    /// <summary>
    /// Whether to enable device detection
    /// </summary>
    public bool EnableDeviceDetection { get; set; } = true;

    /// <summary>
    /// Number of days to retain analytics data
    /// </summary>
    [Range(1, 3650)] // 1 day to 10 years
    public int RetentionDays { get; set; } = 365;

    /// <summary>
    /// Whether to enable performance tracking
    /// </summary>
    public bool EnablePerformanceTracking { get; set; } = true;

    /// <summary>
    /// Performance threshold in milliseconds for tracking slow operations
    /// </summary>
    [Range(100, 30000)] // 100ms to 30 seconds
    public int PerformanceThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Validates the analytics settings using data annotations and custom logic
    /// </summary>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public void Validate()
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);

        if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
        {
            var errors = string.Join("; ", validationResults.Where(v => v.ErrorMessage != null).Select(v => v.ErrorMessage));
            throw new ValidationException($"Analytics settings validation failed: {errors}");
        }

        // Custom validation for interdependent settings
        if (EnableGeoLocation && RetentionDays < 30)
        {
            throw new ValidationException("Geo-location tracking requires a minimum retention period of 30 days for compliance.");
        }

        if (EnablePerformanceTracking && PerformanceThresholdMs < 100)
        {
            throw new ValidationException("Performance threshold must be at least 100ms when performance tracking is enabled.");
        }
    }
}
