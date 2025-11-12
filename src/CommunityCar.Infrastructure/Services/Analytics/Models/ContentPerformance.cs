using System;
using System.Collections.Generic;

namespace CommunityCar.Infrastructure.Services.Analytics.Models;

/// <summary>
/// Performance metrics for content.
/// </summary>
public class ContentPerformance
{
    /// <summary>
    /// Gets or sets the content identifier.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of views.
    /// </summary>
    public int Views { get; set; }

    /// <summary>
    /// Gets or sets the engagement rate.
    /// </summary>
    public double EngagementRate { get; set; }

    /// <summary>
    /// Gets or sets the average time spent on the content.
    /// </summary>
    public TimeSpan AverageTimeSpent { get; set; }

    /// <summary>
    /// Gets or sets the bounce rate.
    /// </summary>
    public double BounceRate { get; set; }
}

/// <summary>
/// Trending content information.
/// </summary>
public class TrendingContent
{
    /// <summary>
    /// Gets or sets the content identifier.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trend direction.
    /// </summary>
    public string TrendDirection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the change percentage.
    /// </summary>
    public double ChangePercentage { get; set; }
}

/// <summary>
/// Content performance report.
/// </summary>
public class ContentPerformanceReport
{
    /// <summary>
    /// Gets or sets the report period.
    /// </summary>
    public DateRange ReportPeriod { get; set; } = new();

    /// <summary>
    /// Gets or sets the top performing content.
    /// </summary>
    public List<ContentPerformance> TopPerformingContent { get; set; } = new();

    /// <summary>
    /// Gets or sets the content type breakdown.
    /// </summary>
    public Dictionary<string, int> ContentTypeBreakdown { get; set; } = new();

    /// <summary>
    /// Gets or sets the trending content.
    /// </summary>
    public List<TrendingContent> TrendingContent { get; set; } = new();
}
