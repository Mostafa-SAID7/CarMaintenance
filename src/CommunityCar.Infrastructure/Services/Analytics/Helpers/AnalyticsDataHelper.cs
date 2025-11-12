using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityCar.Infrastructure.Services.Analytics.Helpers;

/// <summary>
/// Helper class for creating analytics data objects.
/// </summary>
public static class AnalyticsDataHelper
{
    /// <summary>
    /// Creates an analytics summary from a report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The analytics summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when report is null.</exception>
    public static AnalyticsSummary CreateAnalyticsSummary(AnalyticsReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        return new AnalyticsSummary
        {
            TotalUsers = report.UniqueUsers,
            TotalEvents = report.TotalEvents,
            MostPopularPage = report.TopPages?.FirstOrDefault().Key ?? "N/A",
            ErrorCount = report.Errors?.Sum(e => e.Count) ?? 0,
            TopUserAction = report.UserActions?.FirstOrDefault().Key ?? "N/A"
        };
    }

    /// <summary>
    /// Creates engagement metrics from a report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The engagement metrics.</returns>
    public static EngagementMetrics CreateEngagementMetrics(AnalyticsReport report)
    {
        return new EngagementMetrics
        {
            TotalUsers = report.UniqueUsers,
            ActiveUsers = report.UniqueUsers, // Simplified
            NewUsers = 0, // Would need historical data
            ReturningUsers = report.UniqueUsers, // Simplified
            AverageSessionDuration = TimeSpan.FromMinutes(AnalyticsConstants.Defaults.AverageSessionDurationMinutes),
            BounceRate = AnalyticsConstants.Defaults.BounceRate,
            PageViewsPerUser = report.TotalEvents / (double)Math.Max(report.UniqueUsers, 1)
        };
    }

    /// <summary>
    /// Creates user segments based on total users.
    /// </summary>
    /// <param name="totalUsers">The total number of users.</param>
    /// <returns>The list of user segments.</returns>
    public static List<UserSegment> CreateUserSegments(int totalUsers)
    {
        return new List<UserSegment>
        {
            new UserSegment
            {
                Name = "Highly Active",
                UserCount = (int)(totalUsers * AnalyticsConstants.UserSegments.HighlyActivePercentage / 100),
                Percentage = AnalyticsConstants.UserSegments.HighlyActivePercentage
            },
            new UserSegment
            {
                Name = "Moderately Active",
                UserCount = (int)(totalUsers * AnalyticsConstants.UserSegments.ModeratelyActivePercentage / 100),
                Percentage = AnalyticsConstants.UserSegments.ModeratelyActivePercentage
            },
            new UserSegment
            {
                Name = "Low Activity",
                UserCount = (int)(totalUsers * AnalyticsConstants.UserSegments.LowActivityPercentage / 100),
                Percentage = AnalyticsConstants.UserSegments.LowActivityPercentage
            }
        };
    }

    /// <summary>
    /// Creates retention data.
    /// </summary>
    /// <returns>The retention data dictionary.</returns>
    public static Dictionary<int, double> CreateRetentionData()
    {
        return new Dictionary<int, double>
        {
            [AnalyticsConstants.RetentionDays.Day1] = AnalyticsConstants.RetentionRates.Day1Rate,
            [AnalyticsConstants.RetentionDays.Day7] = AnalyticsConstants.RetentionRates.Day7Rate,
            [AnalyticsConstants.RetentionDays.Day30] = AnalyticsConstants.RetentionRates.Day30Rate
        };
    }

    /// <summary>
    /// Creates funnel analysis data.
    /// </summary>
    /// <returns>The list of funnel steps.</returns>
    public static List<FunnelStep> CreateFunnelAnalysis()
    {
        return new List<FunnelStep>
        {
            new FunnelStep { Name = "Landing Page", Users = 1000, ConversionRate = 100 },
            new FunnelStep { Name = "Registration", Users = 750, ConversionRate = 75 },
            new FunnelStep { Name = "Profile Setup", Users = 600, ConversionRate = 60 },
            new FunnelStep { Name = "First Post", Users = 400, ConversionRate = 40 },
            new FunnelStep { Name = "Active User", Users = 250, ConversionRate = 25 }
        };
    }
}
