using CommunityCar.Api.Controllers.MainPages;
using CommunityCar.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Api.Controllers.Dashboard;

[ApiController]
[Route("api/widgets")]
[Authorize]
public class WidgetController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IVisitorTrackingService _visitorTrackingService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public WidgetController(
        IAnalyticsService analyticsService,
        IVisitorTrackingService visitorTrackingService,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService)
    {
        _analyticsService = analyticsService;
        _visitorTrackingService = visitorTrackingService;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    [HttpGet("stats-card")]
    [ProducesResponseType(typeof(StatsCardWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatsCard([FromQuery] string metric, [FromQuery] string period = "today")
    {
        var card = await GetStatsCardDataAsync(metric, period);
        return Ok(card);
    }

    [HttpGet("chart")]
    [ProducesResponseType(typeof(ChartWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChart(
        [FromQuery] string type,
        [FromQuery] string metric,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? _dateTimeService.GetStartOfMonth(_dateTimeService.Now);
        var end = endDate ?? _dateTimeService.Now;

        var chart = await GetChartDataAsync(type, metric, start, end);
        return Ok(chart);
    }

    [HttpGet("recent-activity")]
    [ProducesResponseType(typeof(ActivityFeedWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int count = 10)
    {
        var activities = await GetRecentActivitiesAsync(count);
        return Ok(new ActivityFeedWidget
        {
            Activities = activities,
            TotalCount = activities.Count(),
            LastUpdated = _dateTimeService.Now
        });
    }

    [HttpGet("quick-actions")]
    [ProducesResponseType(typeof(QuickActionsWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuickActions()
    {
        var actions = await GetQuickActionsAsync();
        return Ok(new QuickActionsWidget
        {
            Actions = actions,
            UserRole = _currentUserService.GetCurrentUserRoles().FirstOrDefault() ?? "User"
        });
    }

    [HttpGet("notifications")]
    [ProducesResponseType(typeof(NotificationsWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationsWidget([FromQuery] int count = 5)
    {
        var notifications = await GetNotificationsAsync(count);
        return Ok(new NotificationsWidget
        {
            Notifications = notifications,
            UnreadCount = notifications.Count(n => !n.IsRead),
            LastUpdated = _dateTimeService.Now
        });
    }

    [HttpGet("weather")]
    [ProducesResponseType(typeof(WeatherWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeatherWidget()
    {
        var weather = await GetWeatherDataAsync();
        return Ok(weather);
    }

    [HttpGet("calendar-events")]
    [ProducesResponseType(typeof(CalendarWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? _dateTimeService.Now;
        var events = await GetCalendarEventsAsync(targetDate);
        return Ok(new CalendarWidget
        {
            Date = targetDate,
            Events = events,
            TotalEvents = events.Count()
        });
    }

    [HttpGet("progress-tracker")]
    [ProducesResponseType(typeof(ProgressWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgressTracker([FromQuery] string type)
    {
        var progress = await GetProgressDataAsync(type);
        return Ok(progress);
    }

    [HttpGet("top-items")]
    [ProducesResponseType(typeof(TopItemsWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopItems(
        [FromQuery] string category,
        [FromQuery] string metric = "views",
        [FromQuery] int count = 5)
    {
        var items = await GetTopItemsAsync(category, metric, count);
        return Ok(new TopItemsWidget
        {
            Category = category,
            Metric = metric,
            Items = items,
            GeneratedAt = _dateTimeService.Now
        });
    }

    [HttpGet("system-health")]
    [ProducesResponseType(typeof(SystemHealthWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemHealth()
    {
        var health = await GetSystemHealthAsync();
        return Ok(health);
    }

    [HttpGet("user-profile-summary")]
    [ProducesResponseType(typeof(UserProfileWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserProfileSummary()
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await GetUserProfileSummaryAsync(userId);
        return Ok(profile);
    }

    [HttpGet("social-feed")]
    [ProducesResponseType(typeof(SocialFeedWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSocialFeed([FromQuery] int count = 10)
    {
        var feed = await GetSocialFeedAsync(count);
        return Ok(feed);
    }

    [HttpGet("performance-metrics")]
    [ProducesResponseType(typeof(PerformanceWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics()
    {
        var metrics = await GetPerformanceMetricsAsync();
        return Ok(metrics);
    }

    [HttpGet("custom-widget")]
    [ProducesResponseType(typeof(CustomWidget), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomWidget([FromQuery] string widgetId)
    {
        var widget = await GetCustomWidgetAsync(widgetId);
        return Ok(widget);
    }

    // Helper methods (implementations would depend on your services)
    private async Task<StatsCardWidget> GetStatsCardDataAsync(string metric, string period) => new StatsCardWidget();
    private async Task<ChartWidget> GetChartDataAsync(string type, string metric, DateTime start, DateTime end) => new ChartWidget();
    private async Task<IEnumerable<ActivityItem>> GetRecentActivitiesAsync(int count) => new List<ActivityItem>();
    private async Task<IEnumerable<QuickAction>> GetQuickActionsAsync() => new List<QuickAction>();
    private async Task<IEnumerable<NotificationItem>> GetNotificationsAsync(int count) => new List<NotificationItem>();
    private async Task<WeatherWidget> GetWeatherDataAsync() => new WeatherWidget();
    private async Task<IEnumerable<CalendarEvent>> GetCalendarEventsAsync(DateTime date) => new List<CalendarEvent>();
    private async Task<ProgressWidget> GetProgressDataAsync(string type) => new ProgressWidget();
    private async Task<IEnumerable<TopItem>> GetTopItemsAsync(string category, string metric, int count) => new List<TopItem>();
    private async Task<SystemHealthWidget> GetSystemHealthAsync() => new SystemHealthWidget();
    private async Task<UserProfileWidget> GetUserProfileSummaryAsync(string userId) => new UserProfileWidget();
    private async Task<SocialFeedWidget> GetSocialFeedAsync(int count) => new SocialFeedWidget();
    private async Task<PerformanceWidget> GetPerformanceMetricsAsync() => new PerformanceWidget();
    private async Task<CustomWidget> GetCustomWidgetAsync(string widgetId) => new CustomWidget();
}

// Widget data models
public class StatsCardWidget
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty; // "increase", "decrease", "neutral"
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ChartWidget
{
    public string Type { get; set; } = string.Empty; // "line", "bar", "pie", "area"
    public string Title { get; set; } = string.Empty;
    public IEnumerable<string> Labels { get; set; } = new List<string>();
    public IEnumerable<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    public ChartOptions Options { get; set; } = new();
}

public class ChartDataset
{
    public string Label { get; set; } = string.Empty;
    public IEnumerable<object> Data { get; set; } = new List<object>();
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public int BorderWidth { get; set; } = 1;
    public bool Fill { get; set; } = false;
}

public class ChartOptions
{
    public bool Responsive { get; set; } = true;
    public ChartScales Scales { get; set; } = new();
    public ChartPlugins Plugins { get; set; } = new();
}

public class ChartScales
{
    public ChartAxis X { get; set; } = new();
    public ChartAxis Y { get; set; } = new();
}

public class ChartAxis
{
    public bool Display { get; set; } = true;
    public string Title { get; set; } = new();
}

public class ChartPlugins
{
    public ChartLegend Legend { get; set; } = new();
    public ChartTooltip Tooltip { get; set; } = new();
}

public class ChartLegend
{
    public bool Display { get; set; } = true;
    public string Position { get; set; } = "top";
}

public class ChartTooltip
{
    public bool Enabled { get; set; } = true;
    public string Mode { get; set; } = "nearest";
}

public class ActivityFeedWidget
{
    public IEnumerable<ActivityItem> Activities { get; set; } = new List<ActivityItem>();
    public int TotalCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class QuickActionsWidget
{
    public IEnumerable<QuickAction> Actions { get; set; } = new List<QuickAction>();
    public string UserRole { get; set; } = string.Empty;
}

public class QuickAction
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool RequiresConfirmation { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class NotificationsWidget
{
    public IEnumerable<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();
    public int UnreadCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class WeatherWidget
{
    public string Location { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public IEnumerable<WeatherForecast> Forecast { get; set; } = new List<WeatherForecast>();
    public DateTime LastUpdated { get; set; }
}

public class WeatherForecast
{
    public DateTime Date { get; set; }
    public double HighTemp { get; set; }
    public double LowTemp { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double Precipitation { get; set; }
}

public class CalendarWidget
{
    public DateTime Date { get; set; }
    public IEnumerable<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    public int TotalEvents { get; set; }
}

public class CalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public IEnumerable<string> Attendees { get; set; } = new List<string>();
}

public class ProgressWidget
{
    public string Title { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public double Percentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public IEnumerable<ProgressMilestone> Milestones { get; set; } = new List<ProgressMilestone>();
}

public class ProgressMilestone
{
    public string Title { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TopItemsWidget
{
    public string Category { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public IEnumerable<TopItem> Items { get; set; } = new List<TopItem>();
    public DateTime GeneratedAt { get; set; }
}

public class TopItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Rank { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty; // "up", "down", "stable"
    public double ChangePercent { get; set; }
}

public class SystemHealthWidget
{
    public string OverallStatus { get; set; } = string.Empty;
    public double UptimePercentage { get; set; }
    public IEnumerable<SystemComponent> Components { get; set; } = new List<SystemComponent>();
    public IEnumerable<SystemAlert> Alerts { get; set; } = new List<SystemAlert>();
    public DateTime LastChecked { get; set; }
}

public class SystemComponent
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double ResponseTime { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}

public class UserProfileWidget
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int PostsCount { get; set; }
    public int CommentsCount { get; set; }
    public int LikesReceived { get; set; }
    public int ReputationScore { get; set; }
    public string MemberSince { get; set; } = string.Empty;
    public string LastActive { get; set; } = string.Empty;
    public IEnumerable<string> Badges { get; set; } = new List<string>();
    public IEnumerable<string> RecentActivity { get; set; } = new List<string>();
}

public class SocialFeedWidget
{
    public IEnumerable<SocialPost> Posts { get; set; } = new List<SocialPost>();
    public int TotalPosts { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class SocialPost
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public bool IsLiked { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class PerformanceWidget
{
    public double ResponseTime { get; set; }
    public int RequestsPerSecond { get; set; }
    public double ErrorRate { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public IEnumerable<PerformanceMetric> Metrics { get; set; } = new List<PerformanceMetric>();
    public DateTime MeasuredAt { get; set; }
}

public class PerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Threshold { get; set; }
}

public class CustomWidget
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public WidgetConfig Config { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class WidgetConfig
{
    public int RefreshInterval { get; set; } // seconds
    public bool AutoRefresh { get; set; } = true;
    public string Size { get; set; } = "medium"; // small, medium, large
    public Dictionary<string, object> Settings { get; set; } = new();
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}

public static class WidgetExtensions
{
    public static IServiceCollection AddWidgetServices(this IServiceCollection services)
    {
        // Register widget-related services
        return services;
    }

    public static IApplicationBuilder UseWidgetMiddleware(this IApplicationBuilder app)
    {
        // Add widget middleware
        return app;
    }
}
