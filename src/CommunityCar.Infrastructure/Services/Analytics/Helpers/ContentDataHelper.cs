using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityCar.Infrastructure.Services.Analytics.Helpers;

/// <summary>
/// Helper class for creating content-related data objects.
/// </summary>
public static class ContentDataHelper
{
    /// <summary>
    /// Creates top performing content from a report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The list of top performing content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    public static List<ContentPerformance> CreateTopPerformingContent(AnalyticsReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        if (report.TopPages == null || !report.TopPages.Any())
            return new List<ContentPerformance>();

        return report.TopPages
            .Select(kvp => new ContentPerformance
            {
                ContentId = kvp.Key ?? string.Empty,
                Title = GetContentTitle(kvp.Key),
                Views = kvp.Value,
                EngagementRate = AnalyticsConstants.Defaults.EngagementRate,
                AverageTimeSpent = TimeSpan.FromSeconds(AnalyticsConstants.Defaults.AverageTimeSpentSeconds),
                BounceRate = AnalyticsConstants.Defaults.BounceRate
            })
            .ToList();
    }

    /// <summary>
    /// Creates content type breakdown from a report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The content type breakdown dictionary.</returns>
    public static Dictionary<string, int> CreateContentTypeBreakdown(AnalyticsReport report)
    {
        return new Dictionary<string, int>
        {
            ["Posts"] = report.TopPages.Count,
            ["Comments"] = report.UserActions.GetValueOrDefault("comment", 0),
            ["Profiles"] = report.TopPages.Count(p => p.Key.Contains("profile", StringComparison.OrdinalIgnoreCase)),
            ["Other"] = Math.Max(0, report.TotalEvents - report.TopPages.Sum(p => p.Value))
        };
    }

    /// <summary>
    /// Creates trending content from a report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The list of trending content.</returns>
    public static List<TrendingContent> CreateTrendingContent(AnalyticsReport report)
    {
        return report.TopPages
            .Take(5)
            .Select(kvp => new TrendingContent
            {
                ContentId = kvp.Key,
                Title = GetContentTitle(kvp.Key),
                TrendDirection = "up",
                ChangePercentage = AnalyticsConstants.TrendChangePercentage
            })
            .ToList();
    }

    /// <summary>
    /// Gets a human-readable title for content based on URL.
    /// </summary>
    /// <param name="url">The content URL.</param>
    /// <returns>The content title.</returns>
    private static string GetContentTitle(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "Unknown Content";

        // Enhanced content title resolution
        if (url.Contains("post", StringComparison.OrdinalIgnoreCase)) return "Community Post";
        if (url.Contains("profile", StringComparison.OrdinalIgnoreCase)) return "User Profile";
        if (url.Contains("dashboard", StringComparison.OrdinalIgnoreCase)) return "Dashboard";
        if (url.Contains("forum", StringComparison.OrdinalIgnoreCase)) return "Forum";
        if (url.Contains("article", StringComparison.OrdinalIgnoreCase)) return "Article";
        if (url.Contains("blog", StringComparison.OrdinalIgnoreCase)) return "Blog Post";

        return "Content Page";
    }
}
