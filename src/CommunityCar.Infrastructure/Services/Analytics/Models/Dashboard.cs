using System.Collections.Generic;

namespace CommunityCar.Infrastructure.Services.Analytics.Models;

/// <summary>
/// Summary information for analytics.
/// </summary>
public class AnalyticsSummary
{
    /// <summary>
    /// Gets or sets the total number of users.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gets or sets the total number of events.
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Gets or sets the most popular page.
    /// </summary>
    public string MostPopularPage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total error count.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the top user action.
    /// </summary>
    public string TopUserAction { get; set; } = string.Empty;
}

/// <summary>
/// Chart data for analytics.
/// </summary>
public class AnalyticsCharts
{
    /// <summary>
    /// Gets or sets the events over time data.
    /// </summary>
    public Dictionary<DateTime, int> EventsOverTime { get; set; } = new();

    /// <summary>
    /// Gets or sets the top pages data.
    /// </summary>
    public Dictionary<string, int> TopPages { get; set; } = new();

    /// <summary>
    /// Gets or sets the user actions data.
    /// </summary>
    public Dictionary<string, int> UserActions { get; set; } = new();

    /// <summary>
    /// Gets or sets the error trends data.
    /// </summary>
    public Dictionary<DateTime, int> ErrorTrends { get; set; } = new();
}

/// <summary>
/// Dashboard analytics report.
/// </summary>
public class DashboardAnalyticsReport
{
    /// <summary>
    /// Gets or sets the report period.
    /// </summary>
    public DateRange ReportPeriod { get; set; } = new();

    /// <summary>
    /// Gets or sets the analytics summary.
    /// </summary>
    public AnalyticsSummary Summary { get; set; } = new();

    /// <summary>
    /// Gets or sets the analytics charts.
    /// </summary>
    public AnalyticsCharts Charts { get; set; } = new();

    /// <summary>
    /// Gets or sets the insights generated from the data.
    /// </summary>
    public List<string> Insights { get; set; } = new();

    /// <summary>
    /// Gets or sets the recommendations based on the data.
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}
