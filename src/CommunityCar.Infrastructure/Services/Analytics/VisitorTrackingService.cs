using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services;

public class VisitorTrackingService : IVisitorTrackingService
{
    private readonly ILogger<VisitorTrackingService> _logger;
    private readonly ConcurrentDictionary<string, VisitorSession> _activeSessions;
    private readonly ConcurrentDictionary<string, VisitorProfile> _visitorProfiles;
    private readonly IGeoLocationService _geoLocationService;
    private readonly IDeviceDetectionService _deviceDetectionService;
    private readonly IDateTimeService _dateTimeService;

    public VisitorTrackingService(
        ILogger<VisitorTrackingService> logger,
        IGeoLocationService geoLocationService,
        IDeviceDetectionService deviceDetectionService,
        IDateTimeService dateTimeService)
    {
        _logger = logger;
        _activeSessions = new ConcurrentDictionary<string, VisitorSession>();
        _visitorProfiles = new ConcurrentDictionary<string, VisitorProfile>();
        _geoLocationService = geoLocationService;
        _deviceDetectionService = deviceDetectionService;
        _dateTimeService = dateTimeService;
    }

    public async Task TrackPageVisitAsync(PageVisitData visitData)
    {
        try
        {
            // Update or create visitor profile
            var profile = _visitorProfiles.GetOrAdd(visitData.VisitorId, id => new VisitorProfile
            {
                VisitorId = id,
                FirstVisit = visitData.Timestamp,
                LastVisit = visitData.Timestamp,
                TotalVisits = 0,
                TotalPageViews = 0,
                FavoritePages = new List<string>(),
                IsReturningVisitor = false
            });

            // Update profile
            profile.LastVisit = visitData.Timestamp;
            profile.TotalVisits++;
            profile.TotalPageViews++;
            profile.TotalTimeSpent = profile.TotalTimeSpent.Add(visitData.TimeOnPage ?? TimeSpan.Zero);

            // Update favorite pages
            if (!string.IsNullOrEmpty(visitData.PageUrl))
            {
                if (!profile.FavoritePages.Contains(visitData.PageUrl))
                {
                    profile.FavoritePages.Add(visitData.PageUrl);
                }
            }

            // Detect device info
            if (!string.IsNullOrEmpty(visitData.UserAgent))
            {
                var deviceInfo = _deviceDetectionService.DetectDevice(visitData.UserAgent);
                var browserInfo = _deviceDetectionService.DetectBrowser(visitData.UserAgent);
                var osInfo = _deviceDetectionService.DetectOperatingSystem(visitData.UserAgent);

                profile.PreferredBrowser = browserInfo.Name;
                profile.PreferredDeviceType = deviceInfo.Type;
            }

            // Get location info
            if (!string.IsNullOrEmpty(visitData.IpAddress) && visitData.IpAddress != "127.0.0.1" && visitData.IpAddress != "::1")
            {
                try
                {
                    var location = await _geoLocationService.GetLocationAsync(visitData.IpAddress);
                    profile.Country = location.Country;
                    profile.City = location.City;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get location for IP {IpAddress}", visitData.IpAddress);
                }
            }

            // Start or update session
            var sessionId = visitData.SessionId ?? Guid.NewGuid().ToString();
            var session = _activeSessions.GetOrAdd(sessionId, id => new VisitorSession
            {
                SessionId = id,
                VisitorId = visitData.VisitorId,
                UserId = visitData.UserId,
                StartTime = visitData.Timestamp,
                IpAddress = visitData.IpAddress,
                UserAgent = visitData.UserAgent,
                LandingPage = visitData.PageUrl,
                PageViews = 0,
                ActionsPerformed = 0
            });

            session.LastActivity = visitData.Timestamp;
            session.PageViews++;
            session.ExitPage = visitData.PageUrl;

            // Clean up old sessions (older than 30 minutes)
            var cutoffTime = _dateTimeService.Now.AddMinutes(-30);
            var expiredSessions = _activeSessions.Where(s => s.Value.LastActivity < cutoffTime).ToList();
            foreach (var expired in expiredSessions)
            {
                _activeSessions.TryRemove(expired.Key, out _);
            }

            _logger.LogInformation("Tracked page visit for visitor {VisitorId} on page {PageUrl}",
                visitData.VisitorId, visitData.PageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking page visit for visitor {VisitorId}", visitData.VisitorId);
        }
    }

    public async Task TrackUserActionAsync(UserActionData actionData)
    {
        try
        {
            // Update session
            if (_activeSessions.TryGetValue(actionData.SessionId, out var session))
            {
                session.LastActivity = actionData.Timestamp;
                session.ActionsPerformed++;
            }

            // Update visitor profile
            if (_visitorProfiles.TryGetValue(actionData.VisitorId, out var profile))
            {
                profile.LastVisit = actionData.Timestamp;
            }

            _logger.LogInformation("Tracked user action {ActionType} for visitor {VisitorId}",
                actionData.ActionType, actionData.VisitorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user action for visitor {VisitorId}", actionData.VisitorId);
        }
    }

    public async Task<VisitorSession> StartSessionAsync(string visitorId, string ipAddress, string userAgent)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new VisitorSession
        {
            SessionId = sessionId,
            VisitorId = visitorId,
            StartTime = _dateTimeService.Now,
            LastActivity = _dateTimeService.Now,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            PageViews = 0,
            ActionsPerformed = 0
        };

        _activeSessions[sessionId] = session;

        // Detect device info
        var deviceInfo = _deviceDetectionService.DetectDevice(userAgent);
        var browserInfo = _deviceDetectionService.DetectBrowser(userAgent);
        var osInfo = _deviceDetectionService.DetectOperatingSystem(userAgent);

        session.Browser = browserInfo.Name;
        session.OperatingSystem = osInfo.Name;
        session.DeviceType = deviceInfo.Type;

        // Get location info
        if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "127.0.0.1" && ipAddress != "::1")
        {
            try
            {
                var location = await _geoLocationService.GetLocationAsync(ipAddress);
                session.Country = location.Country;
                session.City = location.City;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get location for IP {IpAddress}", ipAddress);
            }
        }

        _logger.LogInformation("Started session {SessionId} for visitor {VisitorId}", sessionId, visitorId);

        return session;
    }

    public async Task EndSessionAsync(string sessionId)
    {
        if (_activeSessions.TryRemove(sessionId, out var session))
        {
            session.EndTime = _dateTimeService.Now;

            _logger.LogInformation("Ended session {SessionId} for visitor {VisitorId} after {Duration}",
                sessionId, session.VisitorId, session.Duration);
        }
    }

    public async Task<VisitorAnalytics> GetVisitorAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var profiles = _visitorProfiles.Values.Where(p =>
                p.LastVisit >= startDate && p.LastVisit <= endDate).ToList();

            var sessions = _activeSessions.Values.Where(s =>
                s.StartTime >= startDate && s.StartTime <= endDate).ToList();

            var analytics = new VisitorAnalytics
            {
                TotalVisitors = profiles.Count,
                UniqueVisitors = profiles.Select(p => p.VisitorId).Distinct().Count(),
                ReturningVisitors = profiles.Count(p => p.IsReturningVisitor),
                TotalPageViews = profiles.Sum(p => p.TotalPageViews),
                TotalSessions = sessions.Count,
                AverageSessionDuration = profiles.Any() && sessions.Any()
                    ? TimeSpan.FromTicks((long)sessions.Average(s => s.Duration.Ticks))
                    : TimeSpan.Zero,
                BounceRate = CalculateBounceRate(sessions),
                PageViewsByUrl = new Dictionary<string, int>(), // Would need to aggregate from actual data
                VisitorsByCountry = profiles.GroupBy(p => p.Country ?? "Unknown")
                                          .ToDictionary(g => g.Key, g => g.Count()),
                VisitorsByDeviceType = profiles.GroupBy(p => p.PreferredDeviceType ?? "Unknown")
                                             .ToDictionary(g => g.Key, g => g.Count()),
                VisitorsByBrowser = profiles.GroupBy(p => p.PreferredBrowser ?? "Unknown")
                                          .ToDictionary(g => g.Key, g => g.Count()),
                TopPages = new List<TopPageData>(), // Would need to aggregate from actual data
                VisitorFlow = new List<VisitorFlowData>(), // Would need to aggregate from actual data
                LastUpdated = _dateTimeService.Now
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visitor analytics");
            return new VisitorAnalytics { LastUpdated = _dateTimeService.Now };
        }
    }

    public async Task<IEnumerable<PageViewData>> GetPageViewsAsync(string pageUrl, DateTime startDate, DateTime endDate)
    {
        // In a real implementation, this would query a database
        // For now, return mock data
        return new List<PageViewData>
        {
            new PageViewData
            {
                PageUrl = pageUrl,
                PageTitle = "Sample Page",
                Views = 100,
                UniqueViews = 80,
                AverageTimeOnPage = TimeSpan.FromSeconds(120),
                BounceRate = 0.3,
                FirstViewed = startDate,
                LastViewed = endDate
            }
        };
    }

    public async Task<IEnumerable<VisitorSession>> GetActiveSessionsAsync()
    {
        return _activeSessions.Values.Where(s => s.IsActive).ToList();
    }

    public async Task<VisitorProfile> GetVisitorProfileAsync(string visitorId)
    {
        return _visitorProfiles.TryGetValue(visitorId, out var profile)
            ? profile
            : new VisitorProfile { VisitorId = visitorId };
    }

    public async Task<bool> IsReturningVisitorAsync(string visitorId)
    {
        return _visitorProfiles.TryGetValue(visitorId, out var profile) && profile.IsReturningVisitor;
    }

    public async Task UpdateVisitorProfileAsync(string visitorId, Dictionary<string, object> properties)
    {
        var profile = await GetVisitorProfileAsync(visitorId);

        foreach (var property in properties)
        {
            // Update profile properties dynamically
            switch (property.Key.ToLower())
            {
                case "country":
                    profile.Country = property.Value?.ToString();
                    break;
                case "city":
                    profile.City = property.Value?.ToString();
                    break;
                case "preferredbrowser":
                    profile.PreferredBrowser = property.Value?.ToString();
                    break;
                case "preferreddevicetype":
                    profile.PreferredDeviceType = property.Value?.ToString();
                    break;
                default:
                    profile.CustomProperties[property.Key] = property.Value;
                    break;
            }
        }

        _visitorProfiles[visitorId] = profile;
    }

    private double CalculateBounceRate(IEnumerable<VisitorSession> sessions)
    {
        if (!sessions.Any()) return 0;

        var bouncedSessions = sessions.Count(s => s.PageViews == 1);
        return (double)bouncedSessions / sessions.Count();
    }
}

// Supporting service implementations
public class GeoLocationService : IGeoLocationService
{
    public async Task<GeoLocationData> GetLocationAsync(string ipAddress)
    {
        // In a real implementation, this would call a geo-location API
        // For now, return mock data
        return new GeoLocationData
        {
            IpAddress = ipAddress,
            Country = "United States",
            CountryCode = "US",
            City = "New York",
            Latitude = 40.7128,
            Longitude = -74.0060,
            TimeZone = "America/New_York",
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<bool> IsValidIpAddressAsync(string ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
}

public class DeviceDetectionService : IDeviceDetectionService
{
    public DeviceInfo DetectDevice(string userAgent)
    {
        // Simple device detection logic
        var ua = userAgent.ToLower();

        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
        {
            return new DeviceInfo { Type = "Mobile", Brand = "Unknown" };
        }
        else if (ua.Contains("tablet") || ua.Contains("ipad"))
        {
            return new DeviceInfo { Type = "Tablet", Brand = "Unknown" };
        }
        else
        {
            return new DeviceInfo { Type = "Desktop", Brand = "Unknown" };
        }
    }

    public BrowserInfo DetectBrowser(string userAgent)
    {
        // Simple browser detection logic
        var ua = userAgent.ToLower();

        if (ua.Contains("chrome"))
        {
            return new BrowserInfo { Name = "Chrome", Version = "Unknown", Engine = "Blink" };
        }
        else if (ua.Contains("firefox"))
        {
            return new BrowserInfo { Name = "Firefox", Version = "Unknown", Engine = "Gecko" };
        }
        else if (ua.Contains("safari") && !ua.Contains("chrome"))
        {
            return new BrowserInfo { Name = "Safari", Version = "Unknown", Engine = "WebKit" };
        }
        else if (ua.Contains("edge"))
        {
            return new BrowserInfo { Name = "Edge", Version = "Unknown", Engine = "Blink" };
        }
        else
        {
            return new BrowserInfo { Name = "Unknown", Version = "Unknown", Engine = "Unknown" };
        }
    }

    public OperatingSystemInfo DetectOperatingSystem(string userAgent)
    {
        // Simple OS detection logic
        var ua = userAgent.ToLower();

        if (ua.Contains("windows"))
        {
            return new OperatingSystemInfo { Name = "Windows", Version = "Unknown", Platform = "Windows" };
        }
        else if (ua.Contains("mac os x") || ua.Contains("macos"))
        {
            return new OperatingSystemInfo { Name = "macOS", Version = "Unknown", Platform = "macOS" };
        }
        else if (ua.Contains("linux"))
        {
            return new OperatingSystemInfo { Name = "Linux", Version = "Unknown", Platform = "Linux" };
        }
        else if (ua.Contains("android"))
        {
            return new OperatingSystemInfo { Name = "Android", Version = "Unknown", Platform = "Android" };
        }
        else if (ua.Contains("ios") || ua.Contains("iphone") || ua.Contains("ipad"))
        {
            return new OperatingSystemInfo { Name = "iOS", Version = "Unknown", Platform = "iOS" };
        }
        else
        {
            return new OperatingSystemInfo { Name = "Unknown", Version = "Unknown", Platform = "Unknown" };
        }
    }
}