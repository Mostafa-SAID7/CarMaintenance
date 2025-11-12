using System;

namespace CommunityCar.Infrastructure.Configurations.Background;

/// <summary>
/// Represents the status of a background job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// The job is queued for processing.
    /// </summary>
    Queued,

    /// <summary>
    /// The job is scheduled for future execution.
    /// </summary>
    Scheduled,

    /// <summary>
    /// The job is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// The job has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The job has failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The job has been cancelled.
    /// </summary>
    Cancelled
}

/// <summary>
/// Contains information about a background job.
/// </summary>
public class JobInfo
{
    /// <summary>
    /// Gets or sets the unique identifier of the job.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type name of the job.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the arguments for the job.
    /// </summary>
    public object? Args { get; set; }

    /// <summary>
    /// Gets or sets the current status of the job.
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the name of the queue the job belongs to.
    /// </summary>
    public string Queue { get; set; } = "default";

    /// <summary>
    /// Gets or sets the date and time when the job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job is scheduled to run.
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job started processing.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the job failed.
    /// </summary>
    public DateTime? FailedAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if the job failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts made for this job.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the job.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets the duration of the job processing.
    /// </summary>
    public TimeSpan? ProcessingDuration =>
        StartedAt.HasValue && CompletedAt.HasValue
            ? CompletedAt.Value - StartedAt.Value
            : null;
}
