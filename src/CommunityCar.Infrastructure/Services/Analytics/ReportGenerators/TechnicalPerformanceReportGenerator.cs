using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using CommunityCar.Infrastructure.Services.Analytics.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;

/// <summary>
/// Generator for technical performance reports.
/// </summary>
public class TechnicalPerformanceReportGenerator : ReportGeneratorBase<TechnicalPerformanceReport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TechnicalPerformanceReportGenerator"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="logger">The logger instance.</param>
    public TechnicalPerformanceReportGenerator(IAnalyticsService analyticsService, ILogger<TechnicalPerformanceReportGenerator> logger)
        : base(analyticsService, logger)
    {
    }

    /// <summary>
    /// Generates the technical performance report implementation.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="analyticsReport">The analytics report data.</param>
    /// <returns>A task representing the asynchronous operation, containing the technical performance report.</returns>
    protected override async Task<TechnicalPerformanceReport> GenerateReportAsync(DateTime startDate, DateTime endDate, AnalyticsReport analyticsReport)
    {
        var report = new TechnicalPerformanceReport
        {
            ReportPeriod = new DateRange { Start = startDate, End = endDate },
            PerformanceMetrics = TechnicalDataHelper.CreatePerformanceMetrics(analyticsReport, startDate, endDate),
            ErrorAnalysis = TechnicalDataHelper.CreateErrorAnalysis(analyticsReport),
            SystemHealth = TechnicalDataHelper.CreateSystemHealthMetrics()
        };

        await Task.CompletedTask; // Ensure async compatibility
        return report;
    }
}
