using System;
using System.Collections.Generic;

namespace CommunityCar.Infrastructure.Services.Analytics.Models;

/// <summary>
/// Metrics for user engagement.
/// </summary>
public class EngagementMetrics
{
    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of active users.
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of new users.
    /// </summary>
    public int NewUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of returning users.
    /// </summary>
    public int ReturningUsers { get; set; }

    /// <summary>
    /// Gets or sets the average session duration.
    /// </summary>
    public TimeSpan AverageSessionDuration { get; set; }

    /// <summary>
    /// Gets or sets the bounce rate.
    /// </summary>
    public double BounceRate { get; set; }

    /// <summary>
    /// Gets or sets the page views per user.
    /// </summary>
    public double PageViewsPerUser { get; set; }
}

/// <summary>
/// Represents a user segment.
/// </summary>
public class UserSegment
{
    /// <summary>
    /// Gets or sets the name of the segment.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of users in this segment.
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// Gets or sets the percentage of total users in this segment.
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Represents a step in the user funnel.
/// </summary>
public class FunnelStep
{
    /// <summary>
    /// Gets or sets the name of the funnel step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of users at this step.
    /// </summary>
    public int Users { get; set; }

    /// <summary>
    /// Gets or sets the conversion rate for this step.
    /// </summary>
    public double ConversionRate { get; set; }
}

/// <summary>
/// User engagement report.
/// </summary>
public class UserEngagementReport
{
    /// <summary>
    /// Gets or sets the report period.
    /// </summary>
    public DateRange ReportPeriod { get; set; } = new();

    /// <summary>
    /// Gets or sets the engagement metrics.
    /// </summary>
    public EngagementMetrics EngagementMetrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the user segments.
    /// </summary>
    public List<UserSegment> UserSegments { get; set; } = new();

    /// <summary>
    /// Gets or sets the retention data.
    /// </summary>
    public Dictionary<int, double> RetentionData { get; set; } = new();

    /// <summary>
    /// Gets or sets the funnel analysis.
    /// </summary>
    public List<FunnelStep> FunnelAnalysis { get; set; } = new();
}
