using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly ConcurrentQueue<AuditEntry> _auditQueue;
    private readonly ConcurrentDictionary<string, List<AuditEntry>> _userActivities;
    private readonly ConcurrentDictionary<string, List<EntityChange>> _entityChanges;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public AuditService(
        ILogger<AuditService> logger,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _logger = logger;
        _auditQueue = new ConcurrentQueue<AuditEntry>();
        _userActivities = new ConcurrentDictionary<string, List<AuditEntry>>();
        _entityChanges = new ConcurrentDictionary<string, List<EntityChange>>();
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;

        // Start the audit processor
        Task.Run(() => ProcessAuditQueueAsync());
    }

    public async Task LogActivityAsync(string action, string resource, string? details = null, Dictionary<string, object>? metadata = null)
    {
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _currentUserService.GetCurrentUserId(),
            UserName = _currentUserService.GetCurrentUserName(),
            Action = action,
            Resource = resource,
            Details = details,
            IpAddress = _currentUserService.GetCurrentUserIpAddress(),
            UserAgent = _currentUserService.GetCurrentUserAgent(),
            Timestamp = _dateTimeService.Now,
            Metadata = metadata ?? new Dictionary<string, object>(),
            SessionId = Guid.NewGuid().ToString() // In real implementation, get from session
        };

        _auditQueue.Enqueue(auditEntry);

        // Add to user activities
        var userActivities = _userActivities.GetOrAdd(auditEntry.UserId ?? "anonymous", _ => new List<AuditEntry>());
        userActivities.Add(auditEntry);

        // Keep only last 1000 entries per user
        if (userActivities.Count > 1000)
        {
            userActivities.RemoveRange(0, userActivities.Count - 1000);
        }

        _logger.LogInformation("Logged audit activity: {Action} on {Resource} by user {UserId}",
            action, resource, auditEntry.UserId);
    }

    public async Task LogEntityChangeAsync(string entityType, string entityId, string action, Dictionary<string, object>? oldValues = null, Dictionary<string, object>? newValues = null)
    {
        var entityChange = new EntityChange
        {
            Id = Guid.NewGuid().ToString(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues ?? new Dictionary<string, object>(),
            NewValues = newValues ?? new Dictionary<string, object>(),
            ChangedBy = _currentUserService.GetCurrentUserId(),
            ChangedByName = _currentUserService.GetCurrentUserName(),
            ChangedAt = _dateTimeService.Now,
            IpAddress = _currentUserService.GetCurrentUserIpAddress()
        };

        // Add to entity changes
        var entityKey = $"{entityType}:{entityId}";
        var changes = _entityChanges.GetOrAdd(entityKey, _ => new List<EntityChange>());
        changes.Add(entityChange);

        // Keep only last 100 changes per entity
        if (changes.Count > 100)
        {
            changes.RemoveRange(0, changes.Count - 100);
        }

        _logger.LogInformation("Logged entity change: {Action} on {EntityType}:{EntityId} by {ChangedBy}",
            action, entityType, entityId, entityChange.ChangedBy);
    }

    public async Task LogSecurityEventAsync(string eventType, string description, string severity = "Info", Dictionary<string, object>? additionalData = null)
    {
        var metadata = new Dictionary<string, object>
        {
            ["event_type"] = eventType,
            ["severity"] = severity,
            ["security_event"] = true
        };

        if (additionalData != null)
        {
            foreach (var item in additionalData)
            {
                metadata[item.Key] = item.Value;
            }
        }

        await LogActivityAsync("security_event", "system", description, metadata);
    }

    public async Task<AuditSummary> GetAuditSummaryAsync(DateTime startDate, DateTime endDate)
    {
        // In a real implementation, this would query a database
        // For now, return summary based on in-memory data
        var allUserActivities = _userActivities.Values.SelectMany(a => a)
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .ToList();

        var summary = new AuditSummary
        {
            TotalEntries = allUserActivities.Count,
            UniqueUsers = allUserActivities.Select(a => a.UserId).Distinct().Count(),
            ActionsByType = allUserActivities.GroupBy(a => a.Action)
                                           .ToDictionary(g => g.Key, g => g.Count()),
            ResourcesByType = allUserActivities.GroupBy(a => a.Resource)
                                             .ToDictionary(g => g.Key, g => g.Count()),
            EntriesByDay = allUserActivities.GroupBy(a => a.Timestamp.Date)
                                          .ToDictionary(g => g.Key, g => g.Count()),
            TopUsers = allUserActivities.GroupBy(a => a.UserId)
                                      .OrderByDescending(g => g.Count())
                                      .Take(10)
                                      .Select(g => new UserActivitySummary
                                      {
                                          UserId = g.Key ?? "unknown",
                                          UserName = g.First().UserName ?? "unknown",
                                          TotalActions = g.Count(),
                                          LastActivity = g.Max(a => a.Timestamp)
                                      })
                                      .ToList(),
            TimeRange = new DateRange { Start = startDate, End = endDate },
            GeneratedAt = _dateTimeService.Now
        };

        return summary;
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditEntriesAsync(
        DateTime startDate,
        DateTime endDate,
        string? userId = null,
        string? action = null,
        string? resource = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _userActivities.Values.SelectMany(a => a)
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (!string.IsNullOrEmpty(resource))
        {
            query = query.Where(a => a.Resource == resource);
        }

        return query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public async Task<IEnumerable<AuditEntry>> GetUserActivitiesAsync(string userId, DateTime startDate, DateTime endDate, int page = 1, int pageSize = 50)
    {
        if (_userActivities.TryGetValue(userId, out var activities))
        {
            return activities
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        return new List<AuditEntry>();
    }

    public async Task<IEnumerable<EntityChange>> GetEntityChangesAsync(string entityType, string entityId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var entityKey = $"{entityType}:{entityId}";
        if (_entityChanges.TryGetValue(entityKey, out var changes))
        {
            var query = changes.AsEnumerable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.ChangedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.ChangedAt <= endDate.Value);
            }

            return query.OrderByDescending(c => c.ChangedAt);
        }

        return new List<EntityChange>();
    }

    public async Task<AuditReport> GenerateAuditReportAsync(AuditReportRequest request)
    {
        var entries = await GetAuditEntriesAsync(
            request.StartDate,
            request.EndDate,
            request.UserId,
            request.Action,
            request.Resource,
            1,
            int.MaxValue);

        var report = new AuditReport
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title ?? "Audit Report",
            Description = request.Description,
            GeneratedAt = _dateTimeService.Now,
            GeneratedBy = _currentUserService.GetCurrentUserId() ?? "system",
            TimeRange = new DateRange { Start = request.StartDate, End = request.EndDate },
            TotalEntries = entries.Count(),
            Entries = entries.ToList(),
            Summary = await GetAuditSummaryAsync(request.StartDate, request.EndDate),
            Filters = new AuditFilters
            {
                UserId = request.UserId,
                Action = request.Action,
                Resource = request.Resource,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            }
        };

        return report;
    }

    public async Task<bool> ExportAuditDataAsync(AuditExportRequest request)
    {
        // In a real implementation, this would export data to various formats
        // For now, just return true
        _logger.LogInformation("Exporting audit data for date range {StartDate} to {EndDate}",
            request.StartDate, request.EndDate);

        return true;
    }

    public async Task<AuditRetentionPolicy> GetRetentionPolicyAsync()
    {
        // In a real implementation, this would be configurable
        return new AuditRetentionPolicy
        {
            RetentionPeriodDays = 365,
            ArchiveAfterDays = 90,
            DeleteAfterDays = 365,
            CompressArchives = true,
            EncryptionEnabled = true
        };
    }

    public async Task<bool> ApplyRetentionPolicyAsync()
    {
        // In a real implementation, this would clean up old audit data
        _logger.LogInformation("Applying audit retention policy");
        return true;
    }

    private async Task ProcessAuditQueueAsync()
    {
        while (true)
        {
            try
            {
                if (_auditQueue.TryDequeue(out var entry))
                {
                    // In a real implementation, save to database
                    _logger.LogDebug("Processed audit entry: {Id}", entry.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audit queue");
            }

            await Task.Delay(100); // Small delay to prevent tight loop
        }
    }
}

// Supporting classes
public class AuditEntry
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class EntityChange
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> OldValues { get; set; } = new();
    public Dictionary<string, object> NewValues { get; set; } = new();
    public string? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? IpAddress { get; set; }
}

public class AuditSummary
{
    public int TotalEntries { get; set; }
    public int UniqueUsers { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
    public Dictionary<string, int> ResourcesByType { get; set; } = new();
    public Dictionary<DateTime, int> EntriesByDay { get; set; } = new();
    public List<UserActivitySummary> TopUsers { get; set; } = new();
    public DateRange TimeRange { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class UserActivitySummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalActions { get; set; }
    public DateTime LastActivity { get; set; }
}

public class AuditReport
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public DateRange TimeRange { get; set; } = new();
    public int TotalEntries { get; set; }
    public List<AuditEntry> Entries { get; set; } = new();
    public AuditSummary Summary { get; set; } = new();
    public AuditFilters Filters { get; set; } = new();
}

public class AuditReportRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public string Format { get; set; } = "json"; // json, csv, pdf, etc.
}

public class AuditExportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public string Format { get; set; } = "csv";
    public string? FileName { get; set; }
}

public class AuditFilters
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AuditRetentionPolicy
{
    public int RetentionPeriodDays { get; set; }
    public int ArchiveAfterDays { get; set; }
    public int DeleteAfterDays { get; set; }
    public bool CompressArchives { get; set; }
    public bool EncryptionEnabled { get; set; }
}