using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using CommunityCar.Infrastructure.Services.Analytics.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;

/// <summary>
/// Generator for content performance reports.
/// </summary>
public class ContentPerformanceReportGenerator : ReportGeneratorBase<ContentPerformanceReport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPerformanceReportGenerator"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="logger">The logger instance.</param>
    public ContentPerformanceReportGenerator(IAnalyticsService analyticsService, ILogger<ContentPerformanceReportGenerator> logger)
        : base(analyticsService, logger)
    {
    }

    /// <summary>
    /// Generates the content performance report implementation.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="analyticsReport">The analytics report data.</param>
    /// <returns>A task representing the asynchronous operation, containing the content performance report.</returns>
    protected override async Task<ContentPerformanceReport> GenerateReportAsync(DateTime startDate, DateTime endDate, AnalyticsReport analyticsReport)
    {
        var report = new ContentPerformanceReport
        {
            ReportPeriod = new DateRange { Start = startDate, End = endDate },
            TopPerformingContent = ContentDataHelper.CreateTopPerformingContent(analyticsReport),
            ContentTypeBreakdown = ContentDataHelper.CreateContentTypeBreakdown(analyticsReport),
            TrendingContent = ContentDataHelper.CreateTrendingContent(analyticsReport)
        };

        await Task.CompletedTask; // Ensure async compatibility
        return report;
    }
}
