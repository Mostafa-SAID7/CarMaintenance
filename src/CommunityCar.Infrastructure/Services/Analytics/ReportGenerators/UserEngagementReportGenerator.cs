using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using CommunityCar.Infrastructure.Services.Analytics.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;

/// <summary>
/// Generator for user engagement reports.
/// </summary>
public class UserEngagementReportGenerator : ReportGeneratorBase<UserEngagementReport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserEngagementReportGenerator"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="logger">The logger instance.</param>
    public UserEngagementReportGenerator(IAnalyticsService analyticsService, ILogger<UserEngagementReportGenerator> logger)
        : base(analyticsService, logger)
    {
    }

    /// <summary>
    /// Generates the user engagement report implementation.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="analyticsReport">The analytics report data.</param>
    /// <returns>A task representing the asynchronous operation, containing the user engagement report.</returns>
    protected override async Task<UserEngagementReport> GenerateReportAsync(DateTime startDate, DateTime endDate, AnalyticsReport analyticsReport)
    {
        var report = new UserEngagementReport
        {
            ReportPeriod = new DateRange { Start = startDate, End = endDate },
            EngagementMetrics = AnalyticsDataHelper.CreateEngagementMetrics(analyticsReport),
            UserSegments = AnalyticsDataHelper.CreateUserSegments(analyticsReport.UniqueUsers),
            RetentionData = AnalyticsDataHelper.CreateRetentionData(),
            FunnelAnalysis = AnalyticsDataHelper.CreateFunnelAnalysis()
        };

        await Task.CompletedTask; // Ensure async compatibility
        return report;
    }
}
