using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using CommunityCar.Infrastructure.Services.Analytics.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;

/// <summary>
/// Generator for dashboard analytics reports.
/// </summary>
public class DashboardReportGenerator : ReportGeneratorBase<DashboardAnalyticsReport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardReportGenerator"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="logger">The logger instance.</param>
    public DashboardReportGenerator(IAnalyticsService analyticsService, ILogger<DashboardReportGenerator> logger)
        : base(analyticsService, logger)
    {
    }

    /// <summary>
    /// Generates the dashboard report implementation.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="analyticsReport">The analytics report data.</param>
    /// <returns>A task representing the asynchronous operation, containing the dashboard report.</returns>
    protected override async Task<DashboardAnalyticsReport> GenerateReportAsync(DateTime startDate, DateTime endDate, AnalyticsReport analyticsReport)
    {
        var report = new DashboardAnalyticsReport
        {
            ReportPeriod = new DateRange { Start = startDate, End = endDate },
            Summary = AnalyticsDataHelper.CreateAnalyticsSummary(analyticsReport),
            Charts = await ChartDataHelper.CreateAnalyticsChartsAsync(startDate, endDate, analyticsReport),
            Insights = InsightGenerator.GenerateInsights(analyticsReport),
            Recommendations = InsightGenerator.GenerateRecommendations(analyticsReport)
        };

        return report;
    }
}
