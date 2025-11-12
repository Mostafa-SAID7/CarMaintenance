namespace CommunityCar.Application.Interfaces;

public interface IVisitorTrackingService
{
    Task TrackPageVisitAsync(PageVisitData visitData);
    Task TrackUserActionAsync(UserActionData actionData);
    Task<VisitorSession> StartSessionAsync(string visitorId, string ipAddress, string userAgent);
    Task EndSessionAsync(string sessionId);
    Task<VisitorAnalytics> GetVisitorAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<PageViewData>> GetPageViewsAsync(string pageUrl, DateTime startDate, DateTime endDate);
    Task<IEnumerable<VisitorSession>> GetActiveSessionsAsync();
    Task<VisitorProfile> GetVisitorProfileAsync(string visitorId);
    Task<bool> IsReturningVisitorAsync(string visitorId);
    Task UpdateVisitorProfileAsync(string visitorId, Dictionary<string, object> properties);
}

public class PageVisitData
{
    public string VisitorId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string ReferrerUrl { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TimeSpan? TimeOnPage { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? DeviceType { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public Dictionary<string, object>? CustomData { get; set; }
}

public class UserActionData
{
    public string VisitorId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
    public string ElementType { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class VisitorSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string VisitorId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.UtcNow - StartTime;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? ReferrerUrl { get; set; }
    public string? LandingPage { get; set; }
    public string? ExitPage { get; set; }
    public int PageViews { get; set; }
    public int ActionsPerformed { get; set; }
    public bool IsActive => !EndTime.HasValue;
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? DeviceType { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public Dictionary<string, object>? CustomData { get; set; }
}

public class VisitorAnalytics
{
    public int TotalVisitors { get; set; }
    public int UniqueVisitors { get; set; }
    public int ReturningVisitors { get; set; }
    public int TotalPageViews { get; set; }
    public int TotalSessions { get; set; }
    public double AverageSessionDuration { get; set; }
    public double BounceRate { get; set; }
    public Dictionary<string, int> PageViewsByUrl { get; set; } = new();
    public Dictionary<string, int> VisitorsByCountry { get; set; } = new();
    public Dictionary<string, int> VisitorsByDeviceType { get; set; } = new();
    public Dictionary<string, int> VisitorsByBrowser { get; set; } = new();
    public List<TopPageData> TopPages { get; set; } = new();
    public List<VisitorFlowData> VisitorFlow { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class PageViewData
{
    public string PageUrl { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public int Views { get; set; }
    public int UniqueViews { get; set; }
    public double AverageTimeOnPage { get; set; }
    public double BounceRate { get; set; }
    public DateTime FirstViewed { get; set; }
    public DateTime LastViewed { get; set; }
}

public class VisitorProfile
{
    public string VisitorId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime FirstVisit { get; set; }
    public DateTime LastVisit { get; set; }
    public int TotalVisits { get; set; }
    public int TotalPageViews { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public List<string> FavoritePages { get; set; } = new();
    public string? PreferredBrowser { get; set; }
    public string? PreferredDeviceType { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool IsReturningVisitor => TotalVisits > 1;
    public Dictionary<string, object>? CustomProperties { get; set; }
}

public class TopPageData
{
    public string PageUrl { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public int Views { get; set; }
    public double AverageTimeOnPage { get; set; }
}

public class VisitorFlowData
{
    public string FromPage { get; set; } = string.Empty;
    public string ToPage { get; set; } = string.Empty;
    public int Transitions { get; set; }
    public double Percentage { get; set; }
}

public interface IGeoLocationService
{
    Task<GeoLocationData> GetLocationAsync(string ipAddress);
    Task<bool> IsValidIpAddressAsync(string ipAddress);
}

public class GeoLocationData
{
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string Isp { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public interface IDeviceDetectionService
{
    DeviceInfo DetectDevice(string userAgent);
    BrowserInfo DetectBrowser(string userAgent);
    OperatingSystemInfo DetectOperatingSystem(string userAgent);
}

public class DeviceInfo
{
    public string Type { get; set; } = "Unknown"; // Desktop, Mobile, Tablet, Bot
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsMobile => Type == "Mobile";
    public bool IsTablet => Type == "Tablet";
    public bool IsDesktop => Type == "Desktop";
    public bool IsBot => Type == "Bot";
}

public class BrowserInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Engine { get; set; } = string.Empty;
    public bool IsMobile { get; set; }
}

public class OperatingSystemInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}

public static class VisitorTrackingExtensions
{
    public static string GenerateVisitorId(this HttpContext context)
    {
        var visitorId = context.Request.Cookies["VisitorId"];
        if (string.IsNullOrEmpty(visitorId))
        {
            visitorId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("VisitorId", visitorId, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
        }
        return visitorId;
    }

    public static string GetSessionId(this HttpContext context)
    {
        var sessionId = context.Items["SessionId"] as string;
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            context.Items["SessionId"] = sessionId;
        }
        return sessionId;
    }

    public static async Task TrackPageVisitAsync(this IVisitorTrackingService service, HttpContext context, string pageTitle = null)
    {
        var visitData = new PageVisitData
        {
            VisitorId = context.GenerateVisitorId(),
            SessionId = context.GetSessionId(),
            PageUrl = context.Request.Path,
            PageTitle = pageTitle ?? context.Request.Path,
            ReferrerUrl = context.Request.Headers.Referer.ToString(),
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            UserId = context.User?.Identity?.Name,
            Timestamp = DateTime.UtcNow
        };

        await service.TrackPageVisitAsync(visitData);
    }
}
