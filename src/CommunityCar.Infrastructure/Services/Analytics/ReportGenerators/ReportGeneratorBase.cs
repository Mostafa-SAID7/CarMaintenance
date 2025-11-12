using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;

/// <summary>
/// Base class for report generators providing common functionality.
/// </summary>
public abstract class ReportGeneratorBase<TReport> : IReportGenerator<TReport>
{
    protected readonly IAnalyticsService AnalyticsService;
    protected readonly ILogger Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportGeneratorBase{TReport}"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="logger">The logger instance.</param>
    protected ReportGeneratorBase(IAnalyticsService analyticsService, ILogger logger)
    {
        AnalyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A task representing the asynchronous operation, containing the report.</returns>
    public async Task<TReport> GenerateAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            Logger.LogInformation("Generating {ReportType} report for period {StartDate} to {EndDate}",
                typeof(TReport).Name, startDate, endDate);

            var analyticsReport = await AnalyticsService.GetAnalyticsReportAsync(startDate, endDate);
            var report = await GenerateReportAsync(startDate, endDate, analyticsReport);

            Logger.LogInformation("{ReportType} report generated successfully", typeof(TReport).Name);
            return report;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating {ReportType} report", typeof(TReport).Name);
            throw;
        }
    }

    /// <summary>
    /// Generates the specific report implementation.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="analyticsReport">The analytics report data.</param>
    /// <returns>A task representing the asynchronous operation, containing the report.</returns>
    protected abstract Task<TReport> GenerateReportAsync(DateTime startDate, DateTime endDate, AnalyticsReport analyticsReport);
}
