namespace CommunityCar.Application.Interfaces;

public interface IBackgroundJobService
{
    Task<string> EnqueueAsync<TJob, TArgs>(TArgs args, string? queue = null, TimeSpan? delay = null)
        where TJob : IBackgroundJob<TArgs>;

    Task<string> ScheduleAsync<TJob, TArgs>(TArgs args, DateTimeOffset enqueueAt, string? queue = null)
        where TJob : IBackgroundJob<TArgs>;

    Task<string> ScheduleRecurringAsync<TJob, TArgs>(
        string jobId,
        TArgs args,
        string cronExpression,
        string? queue = null)
        where TJob : IBackgroundJob<TArgs>;

    Task<bool> DeleteAsync(string jobId);
    Task<bool> RequeueAsync(string jobId);
    Task<JobStatus?> GetJobStatusAsync(string jobId);
    Task<IEnumerable<JobInfo>> GetJobsAsync(string? queue = null, JobStatus? status = null, int count = 100);
    Task<bool> ExistsAsync(string jobId);
    Task<long> GetQueueLengthAsync(string queue);
    Task ClearQueueAsync(string queue);
}

public interface IBackgroundJob<TArgs>
{
    Task ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);
}

public enum JobStatus
{
    Scheduled,
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Deleted,
    Awaiting
}

public class JobInfo
{
    public string JobId { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EnqueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? JobType { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public TimeSpan? ProcessingTime { get; set; }
    public Dictionary<string, object?> JobData { get; set; } = new();
}

public class JobOptions
{
    public string? Queue { get; set; } = "default";
    public TimeSpan? Delay { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan? Timeout { get; set; }
    public bool RemoveOnComplete { get; set; } = true;
    public Dictionary<string, object?> AdditionalData { get; set; } = new();
}

public interface IRecurringJob
{
    string JobId { get; }
    string CronExpression { get; }
    string? Queue { get; }
    bool Enabled { get; set; }
    DateTime? LastExecution { get; }
    DateTime? NextExecution { get; }
    string? TimeZone { get; }
}

public class RecurringJobInfo : JobInfo, IRecurringJob
{
    public string CronExpression { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTime? LastExecution { get; set; }
    public DateTime? NextExecution { get; set; }
    public string? TimeZone { get; set; }
}

// Common background job implementations
public class EmailJob : IBackgroundJob<EmailMessage>
{
    private readonly IEmailService _emailService;

    public EmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task ExecuteAsync(EmailMessage args, CancellationToken cancellationToken = default)
    {
        await _emailService.SendAsync(args);
    }
}

public class NotificationJob : IBackgroundJob<NotificationMessage>
{
    private readonly INotificationService _notificationService;

    public NotificationJob(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task ExecuteAsync(NotificationMessage args, CancellationToken cancellationToken = default)
    {
        await _notificationService.SendNotificationAsync(args);
    }
}

public class DataCleanupJob : IBackgroundJob<DataCleanupArgs>
{
    private readonly IRepository<AuditEntry> _auditRepository;
    private readonly ICacheService _cacheService;

    public DataCleanupJob(IRepository<AuditEntry> auditRepository, ICacheService cacheService)
    {
        _auditRepository = auditRepository;
        _cacheService = cacheService;
    }

    public async Task ExecuteAsync(DataCleanupArgs args, CancellationToken cancellationToken = default)
    {
        // Clean up old audit entries
        var cutoffDate = DateTime.UtcNow.AddDays(-args.RetentionDays);
        var oldEntries = await _auditRepository.FindAsync(e => e.Timestamp < cutoffDate);
        foreach (var entry in oldEntries)
        {
            _auditRepository.Remove(entry);
        }
        await Task.CompletedTask; // Simulate async operation
    }
}

public class NotificationMessage
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
    public Dictionary<string, object?> Data { get; set; } = new();
}

public class DataCleanupArgs
{
    public int RetentionDays { get; set; } = 90;
    public bool IncludeAuditLogs { get; set; } = true;
    public bool IncludeTempFiles { get; set; } = true;
    public bool IncludeExpiredSessions { get; set; } = true;
}

public interface INotificationService
{
    Task SendNotificationAsync(NotificationMessage message);
    Task SendBulkNotificationsAsync(IEnumerable<NotificationMessage> messages);
}