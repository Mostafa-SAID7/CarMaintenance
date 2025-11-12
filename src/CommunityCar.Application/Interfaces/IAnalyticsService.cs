namespace CommunityCar.Application.Interfaces;

public interface IAnalyticsService
{
    Task TrackEventAsync(string eventName, Dictionary<string, object>? properties = null, string? userId = null);
    Task TrackPageViewAsync(string pageName, string? userId = null, Dictionary<string, object>? properties = null);
    Task TrackUserActionAsync(string action, string? userId = null, Dictionary<string, object>? metadata = null);
    Task TrackErrorAsync(Exception exception, string? userId = null, Dictionary<string, object>? context = null);
    Task TrackPerformanceAsync(string operation, TimeSpan duration, string? userId = null, Dictionary<string, object>? metadata = null);
    Task<AnalyticsMetrics> GetMetricsAsync(DateTime startDate, DateTime endDate);
    Task<UserAnalytics> GetUserAnalyticsAsync(string userId, DateTime startDate, DateTime endDate);
    Task<ContentAnalytics> GetContentAnalyticsAsync(string contentId, DateTime startDate, DateTime endDate);
}

public class AnalyticsMetrics
{
    public int TotalEvents { get; set; }
    public int UniqueUsers { get; set; }
    public int PageViews { get; set; }
    public int Errors { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public Dictionary<string, int> TopPages { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class UserAnalytics
{
    public string UserId { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public int PagesViewed { get; set; }
    public int ActionsPerformed { get; set; }
    public List<string> MostVisitedPages { get; set; } = new();
    public List<string> MostPerformedActions { get; set; } = new();
    public DateTime FirstVisit { get; set; }
    public DateTime LastVisit { get; set; }
}

public class ContentAnalytics
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Shares { get; set; }
    public int Comments { get; set; }
    public double AverageRating { get; set; }
    public TimeSpan AverageTimeSpent { get; set; }
    public Dictionary<string, int> ViewsBySource { get; set; } = new();
    public List<ContentInteraction> RecentInteractions { get; set; } = new();
}

public class ContentInteraction
{
    public string UserId { get; set; } = string.Empty;
    public string InteractionType { get; set; } = string.Empty; // view, like, share, comment, rate
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
