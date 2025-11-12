using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics;

/// <summary>
/// Interface for analytics reporting services.
/// </summary>
public interface IAnalyticsReportingService
{
    /// <summary>
    /// Generates a dashboard analytics report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the dashboard report.</returns>
    Task<DashboardAnalyticsReport> GenerateDashboardReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generates a user engagement report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the user engagement report.</returns>
    Task<UserEngagementReport> GenerateUserEngagementReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generates a content performance report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the content performance report.</returns>
    Task<ContentPerformanceReport> GenerateContentPerformanceReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generates a technical performance report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the technical performance report.</returns>
    Task<TechnicalPerformanceReport> GenerateTechnicalPerformanceReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Exports a report in the specified format.
    /// </summary>
    /// <param name="reportType">The type of report to export.</param>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <param name="format">The export format.</param>
    /// <returns>A task representing the asynchronous operation, containing the exportable report.</returns>
    Task<ExportableReport> ExportReportAsync(ReportType reportType, DateTime startDate, DateTime endDate, ExportFormat format);
}
