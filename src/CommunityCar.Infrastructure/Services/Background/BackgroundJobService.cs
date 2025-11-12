using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services.Background;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly ConcurrentDictionary<string, JobInfo> _jobs;
    private readonly ConcurrentDictionary<string, RecurringJobInfo> _recurringJobs;
    private readonly ConcurrentQueue<JobInfo> _jobQueue;
    private readonly SemaphoreSlim _semaphore;

    public BackgroundJobService(ILogger<BackgroundJobService> logger)
    {
        _logger = logger;
        _jobs = new ConcurrentDictionary<string, JobInfo>();
        _recurringJobs = new ConcurrentDictionary<string, RecurringJobInfo>();
        _jobQueue = new ConcurrentQueue<JobInfo>();
        _semaphore = new SemaphoreSlim(1, 1);

        // Start the job processor
        Task.Run(() => ProcessJobsAsync());
    }

    public async Task<string> EnqueueAsync<TJob, TArgs>(TArgs args, string? queue = null, int? delaySeconds = null)
        where TJob : IJob<TArgs>
    {
        var jobId = Guid.NewGuid().ToString();
        var jobInfo = new JobInfo
        {
            Id = jobId,
            Type = typeof(TJob).Name,
            Args = args,
            Status = JobStatus.Queued,
            Queue = queue ?? "default",
            CreatedAt = DateTime.UtcNow,
            ScheduledAt = delaySeconds.HasValue
                ? DateTime.UtcNow.AddSeconds(delaySeconds.Value)
                : DateTime.UtcNow
        };

        _jobs[jobId] = jobInfo;
        _jobQueue.Enqueue(jobInfo);

        _logger.LogInformation("Enqueued job {JobId} of type {JobType}", jobId, typeof(TJob).Name);

        return jobId;
    }

    public async Task<string> ScheduleAsync<TJob, TArgs>(TArgs args, DateTime scheduledTime, string? queue = null)
        where TJob : IJob<TArgs>
    {
        var jobId = Guid.NewGuid().ToString();
        var jobInfo = new JobInfo
        {
            Id = jobId,
            Type = typeof(TJob).Name,
            Args = args,
            Status = JobStatus.Scheduled,
            Queue = queue ?? "default",
            CreatedAt = DateTime.UtcNow,
            ScheduledAt = scheduledTime
        };

        _jobs[jobId] = jobInfo;

        _logger.LogInformation("Scheduled job {JobId} of type {JobType} for {ScheduledTime}",
            jobId, typeof(TJob).Name, scheduledTime);

        return jobId;
    }

    public async Task<string> AddRecurringAsync<TJob, TArgs>(
        string jobId,
        TArgs args,
        string cronExpression,
        string? queue = null)
        where TJob : IJob<TArgs>
    {
        var recurringJob = new RecurringJobInfo
        {
            Id = jobId,
            Type = typeof(TJob).Name,
            Args = args,
            CronExpression = cronExpression,
            Queue = queue ?? "default",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            NextRun = CalculateNextRunTime(cronExpression)
        };

        _recurringJobs[jobId] = recurringJob;

        _logger.LogInformation("Added recurring job {JobId} of type {JobType} with cron {CronExpression}",
            jobId, typeof(TJob).Name, cronExpression);

        return jobId;
    }

    public async Task<bool> CancelAsync(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = JobStatus.Cancelled;
            _logger.LogInformation("Cancelled job {JobId}", jobId);
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteAsync(string jobId)
    {
        var removed = _jobs.TryRemove(jobId, out _);
        if (removed)
        {
            _logger.LogInformation("Deleted job {JobId}", jobId);
        }
        return removed;
    }

    public async Task<bool> RequeueAsync(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = JobStatus.Queued;
            job.CreatedAt = DateTime.UtcNow;
            job.ScheduledAt = DateTime.UtcNow;
            _jobQueue.Enqueue(job);

            _logger.LogInformation("Requeued job {JobId}", jobId);
            return true;
        }

        return false;
    }

    public async Task<JobInfo?> GetJobAsync(string jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return job;
    }

    public async Task<IEnumerable<JobInfo>> GetJobsAsync(
        JobStatus? status = null,
        string? queue = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _jobs.Values.AsEnumerable();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(queue))
        {
            query = query.Where(j => j.Queue == queue);
        }

        return query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<RecurringJobInfo?> GetRecurringJobAsync(string jobId)
    {
        _recurringJobs.TryGetValue(jobId, out var job);
        return job;
    }

    public async Task<IEnumerable<RecurringJobInfo>> GetRecurringJobsAsync()
    {
        return _recurringJobs.Values.OrderBy(j => j.Id);
    }

    public async Task<bool> UpdateRecurringJobAsync(string jobId, RecurringJobInfo updatedJob)
    {
        if (_recurringJobs.ContainsKey(jobId))
        {
            _recurringJobs[jobId] = updatedJob;
            _logger.LogInformation("Updated recurring job {JobId}", jobId);
            return true;
        }

        return false;
    }

    public async Task<bool> RemoveRecurringJobAsync(string jobId)
    {
        var removed = _recurringJobs.TryRemove(jobId, out _);
        if (removed)
        {
            _logger.LogInformation("Removed recurring job {JobId}", jobId);
        }
        return removed;
    }

    public async Task<JobStatistics> GetStatisticsAsync()
    {
        var jobs = _jobs.Values;

        return new JobStatistics
        {
            TotalJobs = jobs.Count(),
            QueuedJobs = jobs.Count(j => j.Status == JobStatus.Queued),
            ProcessingJobs = jobs.Count(j => j.Status == JobStatus.Processing),
            CompletedJobs = jobs.Count(j => j.Status == JobStatus.Completed),
            FailedJobs = jobs.Count(j => j.Status == JobStatus.Failed),
            CancelledJobs = jobs.Count(j => j.Status == JobStatus.Cancelled),
            ScheduledJobs = jobs.Count(j => j.Status == JobStatus.Scheduled),
            AverageProcessingTime = CalculateAverageProcessingTime(jobs),
            JobsByQueue = jobs.GroupBy(j => j.Queue)
                             .ToDictionary(g => g.Key, g => g.Count()),
            JobsByType = jobs.GroupBy(j => j.Type)
                            .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private async Task ProcessJobsAsync()
    {
        while (true)
        {
            try
            {
                await _semaphore.WaitAsync();

                if (_jobQueue.TryDequeue(out var job))
                {
                    if (job.ScheduledAt <= DateTime.UtcNow)
                    {
                        await ProcessJobAsync(job);
                    }
                    else
                    {
                        // Re-queue for later
                        _jobQueue.Enqueue(job);
                    }
                }

                // Process recurring jobs
                await ProcessRecurringJobsAsync();

                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing jobs");
                _semaphore.Release();
            }

            // Wait before next iteration
            await Task.Delay(1000);
        }
    }

    private async Task ProcessJobAsync(JobInfo job)
    {
        try
        {
            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;

            _logger.LogInformation("Processing job {JobId} of type {JobType}", job.Id, job.Type);

            // Here you would resolve and execute the actual job
            // For now, we'll simulate job execution
            await Task.Delay(100); // Simulate work

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Completed job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.Error = ex.Message;
            job.FailedAt = DateTime.UtcNow;

            _logger.LogError(ex, "Failed to process job {JobId}", job.Id);
        }
    }

    private async Task ProcessRecurringJobsAsync()
    {
        foreach (var recurringJob in _recurringJobs.Values.Where(j => j.IsActive))
        {
            if (recurringJob.NextRun <= DateTime.UtcNow)
            {
                // Create a new job instance
                var jobInfo = new JobInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = recurringJob.Type,
                    Args = recurringJob.Args,
                    Status = JobStatus.Queued,
                    Queue = recurringJob.Queue,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledAt = DateTime.UtcNow
                };

                _jobs[jobInfo.Id] = jobInfo;
                _jobQueue.Enqueue(jobInfo);

                // Calculate next run time
                recurringJob.NextRun = CalculateNextRunTime(recurringJob.CronExpression);
                recurringJob.LastRun = DateTime.UtcNow;

                _logger.LogInformation("Triggered recurring job {JobId}", recurringJob.Id);
            }
        }
    }

    private DateTime CalculateNextRunTime(string cronExpression)
    {
        // Simplified cron parsing - in a real implementation, you'd use a proper cron library
        // For now, assume it's a simple interval like "*/5 * * * *" (every 5 minutes)
        return DateTime.UtcNow.AddMinutes(5);
    }

    private TimeSpan CalculateAverageProcessingTime(IEnumerable<JobInfo> jobs)
    {
        var completedJobs = jobs.Where(j => j.Status == JobStatus.Completed &&
                                          j.StartedAt.HasValue && j.CompletedAt.HasValue);

        if (!completedJobs.Any())
            return TimeSpan.Zero;

        var totalTime = completedJobs.Sum(j => (j.CompletedAt!.Value - j.StartedAt!.Value).TotalMilliseconds);
        return TimeSpan.FromMilliseconds(totalTime / completedJobs.Count());
    }
}

// Supporting classes
public class JobInfo
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? Args { get; set; }
    public JobStatus Status { get; set; }
    public string Queue { get; set; } = "default";
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class RecurringJobInfo
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? Args { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public string Queue { get; set; } = "default";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public string? TimeZone { get; set; }
}

public class JobStatistics
{
    public int TotalJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public int CancelledJobs { get; set; }
    public int ScheduledJobs { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public Dictionary<string, int> JobsByQueue { get; set; } = new();
    public Dictionary<string, int> JobsByType { get; set; } = new();
}

public enum JobStatus
{
    Queued,
    Scheduled,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public interface IJob<TArgs>
{
    Task ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);
}

// Example job implementations
public class EmailJob : IJob<EmailJobArgs>
{
    public async Task ExecuteAsync(EmailJobArgs args, CancellationToken cancellationToken = default)
    {
        // Implementation would send email
        await Task.Delay(100, cancellationToken);
    }
}

public class EmailJobArgs
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
}

public class CleanupJob : IJob<CleanupJobArgs>
{
    public async Task ExecuteAsync(CleanupJobArgs args, CancellationToken cancellationToken = default)
    {
        // Implementation would perform cleanup tasks
        await Task.Delay(100, cancellationToken);
    }
}

public class CleanupJobArgs
{
    public string Target { get; set; } = string.Empty;
    public int OlderThanDays { get; set; }
}