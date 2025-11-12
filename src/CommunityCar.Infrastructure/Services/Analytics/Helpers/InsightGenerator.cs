using CommunityCar.Application.Interfaces;
using System.Collections.Generic;

namespace CommunityCar.Infrastructure.Services.Analytics.Helpers;

/// <summary>
/// Helper class for generating insights and recommendations from analytics data.
/// </summary>
public static class InsightGenerator
{
    /// <summary>
    /// Generates insights from an analytics report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The list of insights.</returns>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    public static List<string> GenerateInsights(AnalyticsReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        var insights = new List<string>();

        // User engagement insights
        if (report.UniqueUsers > 100)
            insights.Add("High user engagement with over 100 unique users");
        else if (report.UniqueUsers < 10)
            insights.Add("Low user activity detected - consider user acquisition strategies");

        // Content performance insights
        if (report.TopPages != null && report.TopPages.Count > 0 && report.TopPages.First().Value > 50)
            insights.Add($"Popular content: {report.TopPages.First().Key ?? "Unknown"} has {report.TopPages.First().Value} views");

        // Error insights
        if (report.Errors != null && report.Errors.Count > 0)
        {
            var errorCount = report.Errors.Count;
            insights.Add($"{errorCount} different error type{(errorCount > 1 ? "s" : "")} detected");

            if (report.Errors.Exists(e => e.Count > 10))
                insights.Add("Critical errors with high frequency detected");
        }

        // User action insights
        if (report.UserActions != null)
        {
            if (report.UserActions.ContainsKey("vote") && report.UserActions["vote"] > 20)
                insights.Add("Active community participation with high voting activity");

            if (report.UserActions.ContainsKey("comment") && report.UserActions["comment"] > 30)
                insights.Add("High user engagement through comments");

            if (report.UserActions.ContainsKey("share") && report.UserActions["share"] > 15)
                insights.Add("Viral content sharing behavior detected");

            if (report.UserActions.ContainsKey("search") && report.UserActions["search"] > 50)
                insights.Add("Frequent search usage suggests users are finding content effectively");
        }

        // Traffic insights
        if (report.TotalEvents > 1000)
            insights.Add("High traffic volume indicates strong platform usage");

        return insights;
    }

    /// <summary>
    /// Generates recommendations based on an analytics report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The list of recommendations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    public static List<string> GenerateRecommendations(AnalyticsReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        var recommendations = new List<string>();

        // Error-related recommendations
        if (report.Errors != null && report.Errors.Exists(e => e.Count > 5))
            recommendations.Add("Address frequently occurring errors to improve user experience");

        // Performance recommendations
        if (report.TopPages != null && report.TopPages.Count > 10)
            recommendations.Add("Consider optimizing the most visited pages for better performance");

        // User engagement recommendations
        if (report.UniqueUsers < 50)
            recommendations.Add("Implement user acquisition strategies to increase engagement");

        if (report.TotalEvents < 100)
            recommendations.Add("Consider content marketing strategies to increase user activity");

        // Feature-specific recommendations
        if (report.UserActions != null)
        {
            if (report.UserActions.ContainsKey("search") && report.UserActions["search"] > 30)
                recommendations.Add("Improve search functionality based on high search activity");

            if (report.UserActions.ContainsKey("comment") && report.UserActions["comment"] < 10)
                recommendations.Add("Encourage more user interaction through comments and discussions");

            if (report.UserActions.ContainsKey("share") && report.UserActions["share"] < 5)
                recommendations.Add("Implement social sharing features to increase content visibility");
        }

        return recommendations;
    }
}
