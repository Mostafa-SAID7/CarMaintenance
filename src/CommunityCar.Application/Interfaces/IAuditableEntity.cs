namespace CommunityCar.Application.Interfaces;

public interface IAuditableEntity
{
    string? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    string? LastModifiedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? CreatedByIp { get; set; }
    string? LastModifiedByIp { get; set; }
    int Version { get; set; }
    bool IsDeleted { get; set; }
    string? DeletedBy { get; set; }
    DateTime? DeletedAt { get; set; }
}

public interface IAuditableEntity<TUserKey> : IAuditableEntity
{
    TUserKey? CreatedBy { get; set; }
    TUserKey? LastModifiedBy { get; set; }
    TUserKey? DeletedBy { get; set; }
}

public class AuditEntry
{
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object?> OldValues { get; set; } = new();
    public Dictionary<string, object?> NewValues { get; set; } = new();
    public List<string> ChangedColumns { get; set; } = new();
    public Dictionary<string, object?> AdditionalData { get; set; } = new();
}

public interface IAuditService
{
    Task LogAuditEntryAsync(AuditEntry entry);
    Task<IEnumerable<AuditEntry>> GetAuditTrailAsync(string entityId, string tableName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<AuditEntry>> GetUserAuditTrailAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<AuditSummary> GetAuditSummaryAsync(DateTime startDate, DateTime endDate);
}

public class AuditSummary
{
    public int TotalEntries { get; set; }
    public int InsertOperations { get; set; }
    public int UpdateOperations { get; set; }
    public int DeleteOperations { get; set; }
    public Dictionary<string, int> OperationsByTable { get; set; } = new();
    public Dictionary<string, int> OperationsByUser { get; set; } = new();
    public List<string> MostActiveUsers { get; set; } = new();
    public List<string> MostModifiedTables { get; set; } = new();
}
