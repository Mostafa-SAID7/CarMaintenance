using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.Helpers;

/// <summary>
/// Helper class for generating chart data.
/// </summary>
public static class ChartDataHelper
{
    /// <summary>
    /// Creates analytics charts asynchronously.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="report">The analytics report.</param>
    /// <returns>A task representing the asynchronous operation, containing the analytics charts.</returns>
    public static async Task<AnalyticsCharts> CreateAnalyticsChartsAsync(DateTime startDate, DateTime endDate, AnalyticsReport report)
    {
        return new AnalyticsCharts
        {
            EventsOverTime = await GenerateEventsOverTimeChartAsync(startDate, endDate),
            TopPages = report.TopPages,
            UserActions = report.UserActions,
            ErrorTrends = GenerateErrorTrendsChart(report.Errors)
        };
    }

    /// <summary>
    /// Generates events over time chart data.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A task representing the asynchronous operation, containing the chart data.</returns>
    private static async Task<Dictionary<DateTime, int>> GenerateEventsOverTimeChartAsync(DateTime startDate, DateTime endDate)
    {
        // In a real implementation, this would query actual time-series data
        // For now, return mock data with better distribution
        var result = new Dictionary<DateTime, int>();
        var random = new Random(42); // Fixed seed for consistent results
        var current = startDate.Date;

        while (current <= endDate.Date)
        {
            // Generate more realistic data with some variation
            var baseValue = 100;
            var variation = random.Next(-30, 31);
            result[current] = Math.Max(0, baseValue + variation);
            current = current.AddDays(1);
        }

        await Task.CompletedTask; // Simulate async operation
        return result;
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
