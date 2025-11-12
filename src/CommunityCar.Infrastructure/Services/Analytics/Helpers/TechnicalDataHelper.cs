using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityCar.Infrastructure.Services.Analytics.Helpers;

/// <summary>
/// Helper class for creating technical performance data objects.
/// </summary>
public static class TechnicalDataHelper
{
    /// <summary>
    /// Creates performance metrics from a report and date range.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <param name="startDate">The start date of the period.</param>
    /// <param name="endDate">The end date of the period.</param>
    /// <returns>The performance metrics.</returns>
    public static PerformanceMetrics CreatePerformanceMetrics(AnalyticsReport report, DateTime startDate, DateTime endDate)
    {
        var duration = endDate - startDate;

        return new PerformanceMetrics
        {
            AverageResponseTime = TimeSpan.FromMilliseconds(AnalyticsConstants.Defaults.AverageResponseTimeMs),
            ErrorRate = report.TotalEvents > 0 ? (double)report.Errors.Sum(e => e.Count) / report.TotalEvents : 0,
            UptimePercentage = AnalyticsConstants.Defaults.UptimePercentage,
            Throughput = duration.TotalHours > 0 ? report.TotalEvents / duration.TotalHours : 0,
            PeakConcurrentUsers = AnalyticsConstants.Defaults.PeakConcurrentUsers
        };
    }

    /// <summary>
    /// Creates error analysis from a report.
    /// </summary>
    /// <param name="report">The analytics report.</param>
    /// <returns>The error analysis.</returns>
    public static ErrorAnalysis CreateErrorAnalysis(AnalyticsReport report)
    {
        return new ErrorAnalysis
        {
            TopErrors = report.Errors
                .Select(e => new ErrorDetails
                {
                    ErrorType = e.Type,
                    Message = e.Message,
                    Count = e.Count,
                    AffectedUsers = Math.Max(1, e.Count / 2), // Ensure at least 1 affected user
                    LastOccurred = e.LastOccurred
                })
                .ToList(),
            ErrorTrends = GenerateErrorTrendsChart(report.Errors),
            ErrorCategories = report.Errors
                .GroupBy(e => e.Type)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Count))
        };
    }

    /// <summary>
    /// Creates system health metrics.
    /// </summary>
    /// <returns>The system health metrics.</returns>
    public static SystemHealthMetrics CreateSystemHealthMetrics()
    {
        return new SystemHealthMetrics
        {
            CpuUsage = AnalyticsConstants.Defaults.CpuUsage,
            MemoryUsage = AnalyticsConstants.Defaults.MemoryUsage,
            DiskUsage = AnalyticsConstants.Defaults.DiskUsage,
            DatabaseConnections = AnalyticsConstants.Defaults.DatabaseConnections,
            CacheHitRate = AnalyticsConstants.Defaults.CacheHitRate
        };
    }

    /// <summary>
    /// Generates error trends chart data.
    /// </summary>
    /// <param name="errors">The list of error summaries.</param>
    /// <returns>The error trends dictionary.</returns>
    private static Dictionary<DateTime, int> GenerateErrorTrendsChart(List<ErrorSummary> errors)
    {
        return errors
            .GroupBy(e => e.LastOccurred.Date)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Count));
    }
}
