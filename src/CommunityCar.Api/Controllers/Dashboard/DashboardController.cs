using CommunityCar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Api.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Moderator")]
public class DashboardController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IVisitorTrackingService _visitorTrackingService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAuditService _auditService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(
        IAnalyticsService analyticsService,
        IVisitorTrackingService visitorTrackingService,
        IDateTimeService dateTimeService,
        IAuditService auditService,
        IBackgroundJobService backgroundJobService,
        ICurrentUserService currentUserService)
    {
        _analyticsService = analyticsService;
        _visitorTrackingService = visitorTrackingService;
        _dateTimeService = dateTimeService;
        _auditService = auditService;
        _backgroundJobService = backgroundJobService;
        _currentUserService = currentUserService;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(AdminDashboardOverview), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminOverview([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? _dateTimeService.GetStartOfMonth(_dateTimeService.Now);
        var end = endDate ?? _dateTimeService.Now;

        var overview = new AdminDashboardOverview
        {
            Analytics = await _analyticsService.GetMetricsAsync(start, end),
            SystemMetrics = await GetSystemMetricsAsync(),
            UserManagement = await GetUserManagementStatsAsync(),
            ContentModeration = await GetContentModerationStatsAsync(),
            SecurityOverview = await GetSecurityOverviewAsync(),
            PerformanceMetrics = await GetPerformanceMetricsAsync(),
            RecentActivities = await GetRecentAdminActivitiesAsync(),
            Alerts = await GetAdminAlertsAsync(),
            GeneratedAt = _dateTimeService.Now
        };

        return Ok(overview);
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(DetailedAnalytics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailedAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? metric = null)
    {
        var start = startDate ?? _dateTimeService.GetStartOfMonth(_dateTimeService.Now);
        var end = endDate ?? _dateTimeService.Now;

        var analytics = new DetailedAnalytics
        {
            Overview = await _analyticsService.GetMetricsAsync(start, end),
            UserAnalytics = await GetAllUserAnalyticsAsync(start, end),
            ContentAnalytics = await GetAllContentAnalyticsAsync(start, end),
            TrafficAnalytics = await GetTrafficAnalyticsAsync(start, end),
            ConversionFunnel = await GetConversionFunnelAsync(start, end),
            TimeRange = new DateRange { Start = start, End = end }
        };

        return Ok(analytics);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(UserManagementDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserManagementDashboard(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        var dashboard = new UserManagementDashboard
        {
            TotalUsers = await GetTotalUsersCountAsync(),
            ActiveUsers = await GetActiveUsersCountAsync(),
            NewUsersToday = await GetNewUsersTodayCountAsync(),
            UsersByRole = await GetUsersByRoleAsync(),
            RecentRegistrations = await GetRecentRegistrationsAsync(page, pageSize),
            UserActivityStats = await GetUserActivityStatsAsync(),
            TopContributors = await GetTopContributorsAsync(),
            UserRetentionRates = await GetUserRetentionRatesAsync()
        };

        return Ok(dashboard);
    }

    [HttpGet("content")]
    [ProducesResponseType(typeof(ContentManagementDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContentManagementDashboard()
    {
        var dashboard = new ContentManagementDashboard
        {
            TotalPosts = await GetTotalPostsCountAsync(),
            TotalComments = await GetTotalCommentsCountAsync(),
            TotalForums = await GetTotalForumsCountAsync(),
            TotalGroups = await GetTotalGroupsCountAsync(),
            PendingModeration = await GetPendingModerationCountAsync(),
            ContentByCategory = await GetContentByCategoryAsync(),
            PopularContent = await GetPopularContentAsync(),
            ContentGrowth = await GetContentGrowthStatsAsync(),
            SpamReports = await GetSpamReportsAsync()
        };

        return Ok(dashboard);
    }

    [HttpGet("security")]
    [ProducesResponseType(typeof(SecurityDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecurityDashboard()
    {
        var dashboard = new SecurityDashboard
        {
            FailedLoginAttempts = await GetFailedLoginAttemptsAsync(),
            SuspiciousActivities = await GetSuspiciousActivitiesAsync(),
            BlockedIPs = await GetBlockedIPsAsync(),
            SecurityIncidents = await GetSecurityIncidentsAsync(),
            PasswordResetRequests = await GetPasswordResetRequestsAsync(),
            TwoFactorAuthStats = await GetTwoFactorAuthStatsAsync(),
            AuditLogs = await GetRecentAuditLogsAsync(),
            SecurityRecommendations = GetSecurityRecommendations()
        };

        return Ok(dashboard);
    }

    [HttpGet("performance")]
    [ProducesResponseType(typeof(PerformanceDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceDashboard()
    {
        var dashboard = new PerformanceDashboard
        {
            ResponseTimeMetrics = await GetResponseTimeMetricsAsync(),
            DatabasePerformance = await GetDatabasePerformanceAsync(),
            CachePerformance = await GetCachePerformanceAsync(),
            BackgroundJobs = await GetBackgroundJobsStatusAsync(),
            ErrorRates = await GetErrorRatesAsync(),
            ResourceUsage = GetResourceUsage(),
            SlowQueries = await GetSlowQueriesAsync(),
            Recommendations = GetPerformanceRecommendations()
        };

        return Ok(dashboard);
    }

    [HttpGet("audit")]
    [ProducesResponseType(typeof(AuditDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null)
    {
        var start = startDate ?? _dateTimeService.GetStartOfWeek(_dateTimeService.Now);
        var end = endDate ?? _dateTimeService.Now;

        var dashboard = new AuditDashboard
        {
            Summary = await _auditService.GetAuditSummaryAsync(start, end),
            RecentEntries = await GetRecentAuditEntriesAsync(start, end, userId, action),
            UserActivities = await GetUserActivitiesAsync(start, end),
            EntityChanges = await GetEntityChangesAsync(start, end),
            TimeRange = new DateRange { Start = start, End = end }
        };

        return Ok(dashboard);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(JobDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobDashboard()
    {
        var dashboard = new JobDashboard
        {
            ActiveJobs = await _backgroundJobService.GetJobsAsync(status: JobStatus.Processing),
            FailedJobs = await _backgroundJobService.GetJobsAsync(status: JobStatus.Failed),
            ScheduledJobs = await _backgroundJobService.GetJobsAsync(status: JobStatus.Scheduled),
            RecurringJobs = await GetRecurringJobsAsync(),
            JobStatistics = await GetJobStatisticsAsync(),
            QueueLengths = await GetQueueLengthsAsync()
        };

        return Ok(dashboard);
    }

    [HttpPost("jobs/{jobId}/retry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RetryJob(string jobId)
    {
        var result = await _backgroundJobService.RequeueAsync(jobId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("jobs/{jobId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteJob(string jobId)
    {
        var result = await _backgroundJobService.DeleteAsync(jobId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("reports")]
    [ProducesResponseType(typeof(ReportsDashboard), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsDashboard()
    {
        var dashboard = new ReportsDashboard
        {
            UserReports = await GetUserReportsAsync(),
            ContentReports = await GetContentReportsAsync(),
            SystemReports = await GetSystemReportsAsync(),
            CustomReports = await GetCustomReportsAsync(),
            ScheduledReports = await GetScheduledReportsAsync()
        };

        return Ok(dashboard);
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(SystemSettings), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemSettings()
    {
        var settings = await GetSystemSettingsAsync();
        return Ok(settings);
    }

    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSystemSettings([FromBody] SystemSettings settings)
    {
        await UpdateSystemSettingsAsync(settings);
        return NoContent();
    }

    // Helper methods (implementations would depend on your services)
    private async Task<SystemMetrics> GetSystemMetricsAsync() => new SystemMetrics();
    private async Task<UserManagementStats> GetUserManagementStatsAsync() => new UserManagementStats();
    private async Task<ContentModerationStats> GetContentModerationStatsAsync() => new ContentModerationStats();
    private async Task<SecurityOverview> GetSecurityOverviewAsync() => new SecurityOverview();
    private async Task<PerformanceMetrics> GetPerformanceMetricsAsync() => new PerformanceMetrics();
    private async Task<IEnumerable<AdminActivity>> GetRecentAdminActivitiesAsync() => new List<AdminActivity>();
    private async Task<IEnumerable<AdminAlert>> GetAdminAlertsAsync() => new List<AdminAlert>();
    private async Task<IEnumerable<UserAnalytics>> GetAllUserAnalyticsAsync(DateTime start, DateTime end) => new List<UserAnalytics>();
    private async Task<IEnumerable<ContentAnalytics>> GetAllContentAnalyticsAsync(DateTime start, DateTime end) => new List<ContentAnalytics>();
    private async Task<TrafficAnalytics> GetTrafficAnalyticsAsync(DateTime start, DateTime end) => new TrafficAnalytics();
    private async Task<ConversionFunnel> GetConversionFunnelAsync(DateTime start, DateTime end) => new ConversionFunnel();
    private async Task<int> GetTotalUsersCountAsync() => 0;
    private async Task<int> GetActiveUsersCountAsync() => 0;
    private async Task<int> GetNewUsersTodayCountAsync() => 0;
    private async Task<Dictionary<string, int>> GetUsersByRoleAsync() => new Dictionary<string, int>();
    private async Task<IEnumerable<UserRegistration>> GetRecentRegistrationsAsync(int page, int pageSize) => new List<UserRegistration>();
    private async Task<UserActivityStats> GetUserActivityStatsAsync() => new UserActivityStats();
    private async Task<IEnumerable<TopContributor>> GetTopContributorsAsync() => new List<TopContributor>();
    private async Task<UserRetentionRates> GetUserRetentionRatesAsync() => new UserRetentionRates();
    private async Task<int> GetTotalPostsCountAsync() => 0;
    private async Task<int> GetTotalCommentsCountAsync() => 0;
    private async Task<int> GetTotalForumsCountAsync() => 0;
    private async Task<int> GetTotalGroupsCountAsync() => 0;
    private async Task<int> GetPendingModerationCountAsync() => 0;
    private async Task<Dictionary<string, int>> GetContentByCategoryAsync() => new Dictionary<string, int>();
    private async Task<IEnumerable<PopularContentItem>> GetPopularContentAsync() => new List<PopularContentItem>();
    private async Task<ContentGrowthStats> GetContentGrowthStatsAsync() => new ContentGrowthStats();
    private async Task<IEnumerable<SpamReport>> GetSpamReportsAsync() => new List<SpamReport>();
    private async Task<IEnumerable<FailedLoginAttempt>> GetFailedLoginAttemptsAsync() => new List<FailedLoginAttempt>();
    private async Task<IEnumerable<SuspiciousActivity>> GetSuspiciousActivitiesAsync() => new List<SuspiciousActivity>();
    private async Task<IEnumerable<BlockedIP>> GetBlockedIPsAsync() => new List<BlockedIP>();
    private async Task<IEnumerable<SecurityIncident>> GetSecurityIncidentsAsync() => new List<SecurityIncident>();
    private async Task<IEnumerable<PasswordResetRequest>> GetPasswordResetRequestsAsync() => new List<PasswordResetRequest>();
    private async Task<TwoFactorAuthStats> GetTwoFactorAuthStatsAsync() => new TwoFactorAuthStats();
    private async Task<IEnumerable<AuditEntry>> GetRecentAuditLogsAsync() => new List<AuditEntry>();
    private List<SecurityRecommendation> GetSecurityRecommendations() => new List<SecurityRecommendation>();
    private async Task<ResponseTimeMetrics> GetResponseTimeMetricsAsync() => new ResponseTimeMetrics();
    private async Task<DatabasePerformance> GetDatabasePerformanceAsync() => new DatabasePerformance();
    private async Task<CachePerformance> GetCachePerformanceAsync() => new CachePerformance();
    private async Task<BackgroundJobsStatus> GetBackgroundJobsStatusAsync() => new BackgroundJobsStatus();
    private async Task<ErrorRates> GetErrorRatesAsync() => new ErrorRates();
    private ResourceUsage GetResourceUsage() => new ResourceUsage();
    private async Task<IEnumerable<SlowQuery>> GetSlowQueriesAsync() => new List<SlowQuery>();
    private List<PerformanceRecommendation> GetPerformanceRecommendations() => new List<PerformanceRecommendation>();
    private async Task<IEnumerable<AuditEntry>> GetRecentAuditEntriesAsync(DateTime start, DateTime end, string? userId, string? action) => new List<AuditEntry>();
    private async Task<Dictionary<string, IEnumerable<AuditEntry>>> GetUserActivitiesAsync(DateTime start, DateTime end) => new Dictionary<string, IEnumerable<AuditEntry>>();
    private async Task<Dictionary<string, IEnumerable<EntityChange>>> GetEntityChangesAsync(DateTime start, DateTime end) => new Dictionary<string, IEnumerable<EntityChange>>();
    private async Task<IEnumerable<RecurringJobInfo>> GetRecurringJobsAsync() => new List<RecurringJobInfo>();
    private async Task<JobStatistics> GetJobStatisticsAsync() => new JobStatistics();
    private async Task<Dictionary<string, long>> GetQueueLengthsAsync() => new Dictionary<string, long>();
    private async Task<IEnumerable<UserReport>> GetUserReportsAsync() => new List<UserReport>();
    private async Task<IEnumerable<ContentReport>> GetContentReportsAsync() => new List<ContentReport>();
    private async Task<IEnumerable<SystemReport>> GetSystemReportsAsync() => new List<SystemReport>();
    private async Task<IEnumerable<CustomReport>> GetCustomReportsAsync() => new List<CustomReport>();
    private async Task<IEnumerable<ScheduledReport>> GetScheduledReportsAsync() => new List<ScheduledReport>();
    private async Task<SystemSettings> GetSystemSettingsAsync() => new SystemSettings();
    private async Task UpdateSystemSettingsAsync(SystemSettings settings) => Task.CompletedTask;
}

// Additional data models for the admin dashboard
public class AdminDashboardOverview
{
    public AnalyticsMetrics Analytics { get; set; } = new();
    public SystemMetrics SystemMetrics { get; set; } = new();
    public UserManagementStats UserManagement { get; set; } = new();
    public ContentModerationStats ContentModeration { get; set; } = new();
    public SecurityOverview SecurityOverview { get; set; } = new();
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    public IEnumerable<AdminActivity> RecentActivities { get; set; } = new List<AdminActivity>();
    public IEnumerable<AdminAlert> Alerts { get; set; } = new List<AdminAlert>();
    public DateTime GeneratedAt { get; set; }
}

public class DetailedAnalytics
{
    public AnalyticsMetrics Overview { get; set; } = new();
    public IEnumerable<UserAnalytics> UserAnalytics { get; set; } = new List<UserAnalytics>();
    public IEnumerable<ContentAnalytics> ContentAnalytics { get; set; } = new List<ContentAnalytics>();
    public TrafficAnalytics TrafficAnalytics { get; set; } = new();
    public ConversionFunnel ConversionFunnel { get; set; } = new();
    public DateRange TimeRange { get; set; } = new();
}

public class DateRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class SystemMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public long DiskUsage { get; set; }
    public long TotalDiskSpace { get; set; }
    public int ActiveConnections { get; set; }
    public TimeSpan Uptime { get; set; }
    public Dictionary<string, ServiceHealth> ServiceHealth { get; set; } = new();
}

public class ServiceHealth
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}

public class UserManagementStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int BannedUsers { get; set; }
    public Dictionary<string, int> UsersByStatus { get; set; } = new();
}

public class ContentModerationStats
{
    public int PendingPosts { get; set; }
    public int PendingComments { get; set; }
    public int ReportedContent { get; set; }
    public int ModeratedToday { get; set; }
    public Dictionary<string, int> ReportsByType { get; set; } = new();
}

public class SecurityOverview
{
    public int FailedLoginsToday { get; set; }
    public int BlockedIPs { get; set; }
    public int ActiveSessions { get; set; }
    public int SecurityAlerts { get; set; }
    public Dictionary<string, int> ThreatsByType { get; set; } = new();
}

public class AdminActivity
{
    public string Id { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AdminAlert
{
    public string Id { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class TrafficAnalytics
{
    public int TotalVisits { get; set; }
    public int UniqueVisitors { get; set; }
    public double AverageSessionDuration { get; set; }
    public double BounceRate { get; set; }
    public Dictionary<string, int> TrafficBySource { get; set; } = new();
    public Dictionary<string, int> VisitsByHour { get; set; } = new();
}

public class ConversionFunnel
{
    public int Visitors { get; set; }
    public int Registrations { get; set; }
    public int ActiveUsers { get; set; }
    public int PayingUsers { get; set; }
    public Dictionary<string, double> ConversionRates { get; set; } = new();
}

public class UserManagementDashboard
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public IEnumerable<UserRegistration> RecentRegistrations { get; set; } = new List<UserRegistration>();
    public UserActivityStats UserActivityStats { get; set; } = new();
    public IEnumerable<TopContributor> TopContributors { get; set; } = new List<TopContributor>();
    public UserRetentionRates UserRetentionRates { get; set; } = new();
}

public class UserRegistration
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public string RegistrationSource { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
}

public class UserActivityStats
{
    public int DailyActiveUsers { get; set; }
    public int WeeklyActiveUsers { get; set; }
    public int MonthlyActiveUsers { get; set; }
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> ActivityByHour { get; set; } = new();
}

public class TopContributor
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int PostsCount { get; set; }
    public int CommentsCount { get; set; }
    public int LikesReceived { get; set; }
    public int ReputationScore { get; set; }
}

public class UserRetentionRates
{
    public double Day1Retention { get; set; }
    public double Day7Retention { get; set; }
    public double Day30Retention { get; set; }
    public double Month1Retention { get; set; }
    public double Month3Retention { get; set; }
}

public class ContentManagementDashboard
{
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalForums { get; set; }
    public int TotalGroups { get; set; }
    public int PendingModeration { get; set; }
    public Dictionary<string, int> ContentByCategory { get; set; } = new();
    public IEnumerable<PopularContentItem> PopularContent { get; set; } = new List<PopularContentItem>();
    public ContentGrowthStats ContentGrowth { get; set; } = new();
    public IEnumerable<SpamReport> SpamReports { get; set; } = new List<SpamReport>();
}

public class PopularContentItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public double EngagementRate { get; set; }
}

public class ContentGrowthStats
{
    public int PostsThisWeek { get; set; }
    public int PostsLastWeek { get; set; }
    public double PostGrowthRate { get; set; }
    public int CommentsThisWeek { get; set; }
    public int CommentsLastWeek { get; set; }
    public double CommentGrowthRate { get; set; }
    public Dictionary<string, int> GrowthByCategory { get; set; } = new();
}

public class SpamReport
{
    public string Id { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SecurityDashboard
{
    public IEnumerable<FailedLoginAttempt> FailedLoginAttempts { get; set; } = new List<FailedLoginAttempt>();
    public IEnumerable<SuspiciousActivity> SuspiciousActivities { get; set; } = new List<SuspiciousActivity>();
    public IEnumerable<BlockedIP> BlockedIPs { get; set; } = new List<BlockedIP>();
    public IEnumerable<SecurityIncident> SecurityIncidents { get; set; } = new List<SecurityIncident>();
    public IEnumerable<PasswordResetRequest> PasswordResetRequests { get; set; } = new List<PasswordResetRequest>();
    public TwoFactorAuthStats TwoFactorAuthStats { get; set; } = new();
    public IEnumerable<AuditEntry> AuditLogs { get; set; } = new List<AuditEntry>();
    public IEnumerable<SecurityRecommendation> SecurityRecommendations { get; set; } = new List<SecurityRecommendation>();
}

public class FailedLoginAttempt
{
    public string IpAddress { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public bool WasSuccessful { get; set; }
}

public class SuspiciousActivity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public int Severity { get; set; }
}

public class BlockedIP
{
    public string IpAddress { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime BlockedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string BlockedBy { get; set; } = string.Empty;
}

public class SecurityIncident
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}

public class PasswordResetRequest
{
    public string Email { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public bool WasSuccessful { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}

public class TwoFactorAuthStats
{
    public int TotalUsersWith2FA { get; set; }
    public int TotalUsers { get; set; }
    public double AdoptionRate { get; set; }
    public Dictionary<string, int> MethodsByType { get; set; } = new();
}

public class SecurityRecommendation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsImplemented { get; set; }
}

public class PerformanceDashboard
{
    public ResponseTimeMetrics ResponseTimeMetrics { get; set; } = new();
    public DatabasePerformance DatabasePerformance { get; set; } = new();
    public CachePerformance CachePerformance { get; set; } = new();
    public BackgroundJobsStatus BackgroundJobs { get; set; } = new();
    public ErrorRates ErrorRates { get; set; } = new();
    public ResourceUsage ResourceUsage { get; set; } = new();
    public IEnumerable<SlowQuery> SlowQueries { get; set; } = new List<SlowQuery>();
    public IEnumerable<PerformanceRecommendation> Recommendations { get; set; } = new List<PerformanceRecommendation>();
}

public class ResponseTimeMetrics
{
    public double AverageResponseTime { get; set; }
    public double Percentile95ResponseTime { get; set; }
    public double Percentile99ResponseTime { get; set; }
    public int RequestsPerSecond { get; set; }
    public Dictionary<string, double> ResponseTimesByEndpoint { get; set; } = new();
}

public class DatabasePerformance
{
    public int ActiveConnections { get; set; }
    public double AverageQueryTime { get; set; }
    public int SlowQueriesCount { get; set; }
    public long CacheHitRatio { get; set; }
    public Dictionary<string, int> QueriesByType { get; set; } = new();
}

public class CachePerformance
{
    public double HitRate { get; set; }
    public int TotalRequests { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public long MemoryUsage { get; set; }
    public Dictionary<string, double> HitRatesByType { get; set; } = new();
}

public class BackgroundJobsStatus
{
    public int ActiveJobs { get; set; }
    public int FailedJobs { get; set; }
    public int ScheduledJobs { get; set; }
    public int CompletedJobsToday { get; set; }
    public double AverageJobDuration { get; set; }
    public Dictionary<string, int> JobsByType { get; set; } = new();
}

public class ErrorRates
{
    public double OverallErrorRate { get; set; }
    public int ErrorsLastHour { get; set; }
    public int ErrorsLast24Hours { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByEndpoint { get; set; } = new();
}

public class ResourceUsage
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public long DiskUsage { get; set; }
    public int ActiveThreads { get; set; }
    public int NetworkConnections { get; set; }
    public Dictionary<string, double> UsageByComponent { get; set; } = new();
}

public class SlowQuery
{
    public string Query { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class PerformanceRecommendation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Effort { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsImplemented { get; set; }
}

public class AuditDashboard
{
    public AuditSummary Summary { get; set; } = new();
    public IEnumerable<AuditEntry> RecentEntries { get; set; } = new List<AuditEntry>();
    public Dictionary<string, IEnumerable<AuditEntry>> UserActivities { get; set; } = new();
    public Dictionary<string, IEnumerable<EntityChange>> EntityChanges { get; set; } = new();
    public DateRange TimeRange { get; set; } = new();
}

public class EntityChange
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> OldValues { get; set; } = new();
    public Dictionary<string, object> NewValues { get; set; } = new();
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}

public class JobDashboard
{
    public IEnumerable<JobInfo> ActiveJobs { get; set; } = new List<JobInfo>();
    public IEnumerable<JobInfo> FailedJobs { get; set; } = new List<JobInfo>();
    public IEnumerable<JobInfo> ScheduledJobs { get; set; } = new List<JobInfo>();
    public IEnumerable<RecurringJobInfo> RecurringJobs { get; set; } = new List<RecurringJobInfo>();
    public JobStatistics JobStatistics { get; set; } = new();
    public Dictionary<string, long> QueueLengths { get; set; } = new();
}

public class JobStatistics
{
    public int TotalJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageJobDuration { get; set; }
    public Dictionary<string, int> JobsByStatus { get; set; } = new();
}

public class ReportsDashboard
{
    public IEnumerable<UserReport> UserReports { get; set; } = new List<UserReport>();
    public IEnumerable<ContentReport> ContentReports { get; set; } = new List<ContentReport>();
    public IEnumerable<SystemReport> SystemReports { get; set; } = new List<SystemReport>();
    public IEnumerable<CustomReport> CustomReports { get; set; } = new List<CustomReport>();
    public IEnumerable<ScheduledReport> ScheduledReports { get; set; } = new List<ScheduledReport>();
}

public class UserReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ContentReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class SystemReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CustomReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ScheduledReport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public bool IsActive { get; set; }
    public string Recipients { get; set; } = string.Empty;
}

public class SystemSettings
{
    public GeneralSettings General { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public AnalyticsSettings Analytics { get; set; } = new();
    public ModerationSettings Moderation { get; set; } = new();
}

public class GeneralSettings
{
    public string SiteName { get; set; } = string.Empty;
    public string SiteDescription { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = string.Empty;
}

public class SecuritySettings
{
    public bool RequireEmailConfirmation { get; set; }
    public bool RequireTwoFactorAuth { get; set; }
    public int PasswordMinLength { get; set; }
    public bool PasswordRequireUppercase { get; set; }
    public bool PasswordRequireLowercase { get; set; }
    public bool PasswordRequireNumbers { get; set; }
    public bool PasswordRequireSymbols { get; set; }
    public int MaxLoginAttempts { get; set; }
    public TimeSpan LockoutDuration { get; set; }
    public bool EnableIpBlocking { get; set; }
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class CacheSettings
{
    public bool EnableCaching { get; set; }
    public TimeSpan DefaultExpiration { get; set; }
    public int MaxCacheSize { get; set; }
    public string CacheProvider { get; set; } = string.Empty;
}

public class AnalyticsSettings
{
    public bool EnableAnalytics { get; set; }
    public bool TrackUserActivity { get; set; }
    public bool TrackPageViews { get; set; }
    public bool EnableRealTimeAnalytics { get; set; }
    public int DataRetentionDays { get; set; }
}

public class ModerationSettings
{
    public bool EnableAutoModeration { get; set; }
    public bool RequirePostApproval { get; set; }
    public bool RequireCommentApproval { get; set; }
    public int SpamThreshold { get; set; }
    public IEnumerable<string> BannedWords { get; set; } = new List<string>();
    public bool EnableImageModeration { get; set; }
}