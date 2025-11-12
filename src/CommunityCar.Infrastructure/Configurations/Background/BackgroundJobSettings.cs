using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Infrastructure.Configurations.Background;

/// <summary>
/// Configuration settings for background job processing.
/// </summary>
public class BackgroundJobSettings
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent jobs that can be processed simultaneously.
    /// Default is 5.
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrentJobs { get; set; } = 5;

    /// <summary>
    /// Gets or sets the timeout in minutes for individual job execution.
    /// Default is 30 minutes.
    /// </summary>
    [Range(1, 1440)] // 1 minute to 24 hours
    public int JobTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed jobs.
    /// Default is 3.
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in seconds between retry attempts.
    /// Default is 60 seconds.
    /// </summary>
    [Range(1, 3600)] // 1 second to 1 hour
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether job persistence is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableJobPersistence { get; set; } = true;

    /// <summary>
    /// Gets or sets the default queue name for jobs.
    /// Default is "default".
    /// </summary>
    [Required]
    [StringLength(50)]
    public string DefaultQueue { get; set; } = "default";

    /// <summary>
    /// Gets or sets the interval in seconds between queue processing cycles.
    /// Default is 5 seconds.
    /// </summary>
    [Range(1, 300)] // 1 second to 5 minutes
    public int QueueProcessingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether recurring jobs are enabled.
    /// Default is true.
    /// </summary>
    public bool EnableRecurringJobs { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of jobs allowed per queue.
    /// Default is 1000.
    /// </summary>
    [Range(1, 10000)]
    public int MaxJobsPerQueue { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum processing time in milliseconds before a job is considered stuck.
    /// Default is 3600000 (1 hour).
    /// </summary>
    [Range(60000, 86400000)] // 1 minute to 24 hours
    public int MaxJobProcessingTimeMs { get; set; } = 3600000;
}
