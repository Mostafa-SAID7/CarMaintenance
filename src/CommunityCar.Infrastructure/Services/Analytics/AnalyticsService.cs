using CommunityCar.Application.Interfaces;
using CommunityCar.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IRepository<AnalyticsEvent> _eventRepository;
    private readonly IRepository<UserAnalytics> _userAnalyticsRepository;
    private readonly IRepository<ContentAnalytics> _contentAnalyticsRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AnalyticsService> _logger;
    private readonly ConcurrentDictionary<string, AnalyticsMetrics> _cache;

    public AnalyticsService(
        IRepository<AnalyticsEvent> eventRepository,
        IRepository<UserAnalytics> userAnalyticsRepository,
        IRepository<ContentAnalytics> contentAnalyticsRepository,
        IDateTimeService dateTimeService,
        ILogger<AnalyticsService> logger)
    {
        _eventRepository = eventRepository;
        _userAnalyticsRepository = userAnalyticsRepository;
        _contentAnalyticsRepository = contentAnalyticsRepository;
        _dateTimeService = dateTimeService;
        _logger = logger;
        _cache = new ConcurrentDictionary<string, AnalyticsMetrics>();
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object>? properties = null, string? userId = null)
    {
        try
        {
            var analyticsEvent = new AnalyticsEvent
            {
                EventName = eventName,
                UserId = userId,
                Properties = properties ?? new Dictionary<string, object>(),
                Timestamp = _dateTimeService.Now,
                SessionId = Guid.NewGuid().ToString(), // In real implementation, get from session
                IpAddress = "127.0.0.1", // In real implementation, get from HTTP context
                UserAgent = "Unknown" // In real implementation, get from HTTP context
            };

            await _eventRepository.AddAsync(analyticsEvent);

            _logger.LogInformation("Tracked analytics event: {EventName} for user {UserId}", eventName, userId);

            // Invalidate cache
            _cache.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking analytics event: {EventName}", eventName);
        }
    }

    public async Task TrackPageViewAsync(string pageName, string? userId = null, Dictionary<string, object>? properties = null)
    {
        var eventProperties = new Dictionary<string, object>
        {
            ["page"] = pageName,
            ["view_type"] = "page"
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                eventProperties[prop.Key] = prop.Value;
            }
        }

        await TrackEventAsync("page_view", eventProperties, userId);
    }

    public async Task TrackUserActionAsync(string action, string? userId = null, Dictionary<string, object>? metadata = null)
    {
        var eventProperties = new Dictionary<string, object>
        {
            ["action"] = action,
            ["action_type"] = "user_interaction"
        };

        if (metadata != null)
        {
            foreach (var item in metadata)
            {
                eventProperties[item.Key] = item.Value;
            }
        }

        await TrackEventAsync("user_action", eventProperties, userId);
    }

    public async Task TrackErrorAsync(Exception exception, string? userId = null, Dictionary<string, object>? context = null)
    {
        var eventProperties = new Dictionary<string, object>
        {
            ["error_type"] = exception.GetType().Name,
            ["error_message"] = exception.Message,
            ["stack_trace"] = exception.StackTrace ?? "No stack trace available",
            ["error_source"] = exception.Source ?? "Unknown"
        };

        if (context != null)
        {
            foreach (var item in context)
            {
                eventProperties[$"context_{item.Key}"] = item.Value;
            }
        }

        await TrackEventAsync("error", eventProperties, userId);
    }

    public async Task TrackPerformanceAsync(string operation, TimeSpan duration, string? userId = null, Dictionary<string, object>? metadata = null)
    {
        var eventProperties = new Dictionary<string, object>
        {
            ["operation"] = operation,
            ["duration_ms"] = duration.TotalMilliseconds,
            ["duration_seconds"] = duration.TotalSeconds,
            ["performance_type"] = "operation_timing"
        };

        if (metadata != null)
        {
            foreach (var item in metadata)
            {
                eventProperties[item.Key] = item.Value;
            }
        }

        await TrackEventAsync("performance", eventProperties, userId);
    }

    public async Task<AnalyticsMetrics> GetMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var cacheKey = $"metrics_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out var cachedMetrics))
        {
            return cachedMetrics;
        }

        try
        {
            var events = await _eventRepository.FindAsync(e =>
                e.Timestamp >= startDate && e.Timestamp <= endDate);

            var metrics = new AnalyticsMetrics
            {
                TotalEvents = events.Count(),
                UniqueUsers = events.Where(e => !string.IsNullOrEmpty(e.UserId))
                                   .Select(e => e.UserId)
                                   .Distinct()
                                   .Count(),
                PageViews = events.Count(e => e.EventName == "page_view"),
                Errors = events.Count(e => e.EventName == "error"),
                AverageSessionDuration = CalculateAverageSessionDuration(events),
                EventsByType = events.GroupBy(e => e.EventName)
                                    .ToDictionary(g => g.Key, g => g.Count()),
                TopPages = events.Where(e => e.EventName == "page_view")
                                .GroupBy(e => e.Properties.GetValueOrDefault("page", "unknown"))
                                .OrderByDescending(g => g.Count())
                                .Take(10)
                                .ToDictionary(g => g.Key?.ToString() ?? "unknown", g => g.Count()),
                ErrorsByType = events.Where(e => e.EventName == "error")
                                    .GroupBy(e => e.Properties.GetValueOrDefault("error_type", "unknown"))
                                    .ToDictionary(g => g.Key?.ToString() ?? "unknown", g => g.Count()),
                LastUpdated = _dateTimeService.Now
            };

            _cache.TryAdd(cacheKey, metrics);
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics metrics");
            return new AnalyticsMetrics { LastUpdated = _dateTimeService.Now };
        }
    }

    public async Task<UserAnalytics> GetUserAnalyticsAsync(string userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var userEvents = await _eventRepository.FindAsync(e =>
                e.UserId == userId &&
                e.Timestamp >= startDate &&
                e.Timestamp <= endDate);

            var sessions = userEvents
                .Where(e => e.SessionId != null)
                .GroupBy(e => e.SessionId)
                .Select(g => new
                {
                    SessionId = g.Key,
                    StartTime = g.Min(e => e.Timestamp),
                    EndTime = g.Max(e => e.Timestamp),
                    Events = g.Count()
                })
                .ToList();

            return new UserAnalytics
            {
                TotalSessions = sessions.Count,
                TotalTimeSpent = sessions.Any()
                    ? TimeSpan.FromTicks((long)sessions.Average(s => (s.EndTime - s.StartTime).Ticks))
                    : TimeSpan.Zero,
                PagesViewed = userEvents.Count(e => e.EventName == "page_view"),
                ActionsPerformed = userEvents.Count(e => e.EventName == "user_action"),
                FavoritePages = userEvents.Where(e => e.EventName == "page_view")
                                         .GroupBy(e => e.Properties.GetValueOrDefault("page", "unknown"))
                                         .OrderByDescending(g => g.Count())
                                         .Take(5)
                                         .Select(g => g.Key?.ToString() ?? "unknown")
                                         .ToList(),
                MostPerformedActions = userEvents.Where(e => e.EventName == "user_action")
                                                .GroupBy(e => e.Properties.GetValueOrDefault("action", "unknown"))
                                                .OrderByDescending(g => g.Count())
                                                .Take(5)
                                                .Select(g => g.Key?.ToString() ?? "unknown")
                                                .ToList(),
                FirstVisit = userEvents.Min(e => e.Timestamp),
                LastVisit = userEvents.Max(e => e.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user analytics for user {UserId}", userId);
            return new UserAnalytics();
        }
    }

    public async Task<ContentAnalytics> GetContentAnalyticsAsync(string contentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var contentEvents = await _eventRepository.FindAsync(e =>
                e.Properties.ContainsKey("content_id") &&
                e.Properties["content_id"]?.ToString() == contentId &&
                e.Timestamp >= startDate &&
                e.Timestamp <= endDate);

            var interactions = contentEvents
                .Where(e => e.EventName == "user_action")
                .Select(e => new ContentInteraction
                {
                    UserId = e.UserId ?? "anonymous",
                    InteractionType = e.Properties.GetValueOrDefault("action", "unknown")?.ToString() ?? "unknown",
                    Timestamp = e.Timestamp,
                    Metadata = e.Properties.Where(p => p.Key != "action").ToDictionary(p => p.Key, p => p.Value)
                })
                .OrderByDescending(i => i.Timestamp)
                .Take(50)
                .ToList();

            return new ContentAnalytics
            {
                ContentId = contentId,
                ContentType = "unknown", // Would be determined by content service
                Views = contentEvents.Count(e => e.EventName == "page_view"),
                Likes = contentEvents.Count(e => e.Properties.GetValueOrDefault("action", "")?.ToString() == "like"),
                Shares = contentEvents.Count(e => e.Properties.GetValueOrDefault("action", "")?.ToString() == "share"),
                Comments = contentEvents.Count(e => e.Properties.GetValueOrDefault("action", "")?.ToString() == "comment"),
                AverageRating = 0, // Would be calculated from rating events
                AverageTimeSpent = TimeSpan.Zero, // Would be calculated from session data
                ViewsBySource = new Dictionary<string, int>(), // Would be populated from referrer data
                RecentInteractions = interactions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving content analytics for content {ContentId}", contentId);
            return new ContentAnalytics { ContentId = contentId };
        }
    }

    private TimeSpan CalculateAverageSessionDuration(IEnumerable<AnalyticsEvent> events)
    {
        var sessions = events
            .Where(e => e.SessionId != null)
            .GroupBy(e => e.SessionId)
            .Select(g => new
            {
                StartTime = g.Min(e => e.Timestamp),
                EndTime = g.Max(e => e.Timestamp)
            })
            .Where(s => s.EndTime > s.StartTime)
            .ToList();

        if (!sessions.Any())
            return TimeSpan.Zero;

        return TimeSpan.FromTicks((long)sessions.Average(s => (s.EndTime - s.StartTime).Ticks));
    }
}

// Domain entities for analytics
public class AnalyticsEvent : BaseEntity
{
    public string EventName { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class UserAnalytics : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public int PagesViewed { get; set; }
    public int ActionsPerformed { get; set; }
    public List<string> FavoritePages { get; set; } = new();
    public List<string> MostPerformedActions { get; set; } = new();
    public DateTime FirstVisit { get; set; }
    public DateTime LastVisit { get; set; }
}

public class ContentAnalytics : BaseEntity
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

public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}