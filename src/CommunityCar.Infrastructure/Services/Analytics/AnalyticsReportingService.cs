using CommunityCar.Application.Interfaces;
using CommunityCar.Infrastructure.Services.Analytics.Models;
using CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;
using CommunityCar.Infrastructure.Services.Analytics.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services;

/// <summary>
/// Improved implementation of the analytics reporting service.
/// Uses composition with specialized report generators for better separation of concerns.
/// </summary>
public class AnalyticsReportingService : IAnalyticsReportingService
{
    private readonly ILogger<AnalyticsReportingService> _logger;
    private readonly IReportGenerator<DashboardAnalyticsReport> _dashboardGenerator;
    private readonly IReportGenerator<UserEngagementReport> _userEngagementGenerator;
    private readonly IReportGenerator<ContentPerformanceReport> _contentPerformanceGenerator;
    private readonly IReportGenerator<TechnicalPerformanceReport> _technicalPerformanceGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsReportingService"/> class.
    /// </summary>
    /// <param name="dashboardGenerator">The dashboard report generator.</param>
    /// <param name="userEngagementGenerator">The user engagement report generator.</param>
    /// <param name="contentPerformanceGenerator">The content performance report generator.</param>
    /// <param name="technicalPerformanceGenerator">The technical performance report generator.</param>
    /// <param name="logger">The logger instance.</param>
    public AnalyticsReportingService(
        IReportGenerator<DashboardAnalyticsReport> dashboardGenerator,
        IReportGenerator<UserEngagementReport> userEngagementGenerator,
        IReportGenerator<ContentPerformanceReport> contentPerformanceGenerator,
        IReportGenerator<TechnicalPerformanceReport> technicalPerformanceGenerator,
        ILogger<AnalyticsReportingService> logger)
    {
        _dashboardGenerator = dashboardGenerator ?? throw new ArgumentNullException(nameof(dashboardGenerator));
        _userEngagementGenerator = userEngagementGenerator ?? throw new ArgumentNullException(nameof(userEngagementGenerator));
        _contentPerformanceGenerator = contentPerformanceGenerator ?? throw new ArgumentNullException(nameof(contentPerformanceGenerator));
        _technicalPerformanceGenerator = technicalPerformanceGenerator ?? throw new ArgumentNullException(nameof(technicalPerformanceGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a dashboard analytics report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the dashboard report.</returns>
    public async Task<DashboardAnalyticsReport> GenerateDashboardReportAsync(DateTime startDate, DateTime endDate)
    {
        DateRangeValidator.Validate(startDate, endDate);

        try
        {
            _logger.LogInformation("Generating dashboard report for period {StartDate} to {EndDate}",
                startDate, endDate);

            var report = await _dashboardGenerator.GenerateAsync(startDate, endDate);

            _logger.LogInformation("Dashboard report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard report");
            throw;
        }
    }

    /// <summary>
    /// Generates a user engagement report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the user engagement report.</returns>
    public async Task<UserEngagementReport> GenerateUserEngagementReportAsync(DateTime startDate, DateTime endDate)
    {
        DateRangeValidator.Validate(startDate, endDate);

        try
        {
            _logger.LogInformation("Generating user engagement report for period {StartDate} to {EndDate}",
                startDate, endDate);

            var report = await _userEngagementGenerator.GenerateAsync(startDate, endDate);

            _logger.LogInformation("User engagement report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user engagement report");
            throw;
        }
    }

    /// <summary>
    /// Generates a content performance report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the content performance report.</returns>
    public async Task<ContentPerformanceReport> GenerateContentPerformanceReportAsync(DateTime startDate, DateTime endDate)
    {
        DateRangeValidator.Validate(startDate, endDate);

        try
        {
            _logger.LogInformation("Generating content performance report for period {StartDate} to {EndDate}",
                startDate, endDate);

            var report = await _contentPerformanceGenerator.GenerateAsync(startDate, endDate);

            _logger.LogInformation("Content performance report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content performance report");
            throw;
        }
    }

    /// <summary>
    /// Generates a technical performance report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <returns>A task representing the asynchronous operation, containing the technical performance report.</returns>
    public async Task<TechnicalPerformanceReport> GenerateTechnicalPerformanceReportAsync(DateTime startDate, DateTime endDate)
    {
        DateRangeValidator.Validate(startDate, endDate);

        try
        {
            _logger.LogInformation("Generating technical performance report for period {StartDate} to {EndDate}",
                startDate, endDate);

            var report = await _technicalPerformanceGenerator.GenerateAsync(startDate, endDate);

            _logger.LogInformation("Technical performance report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating technical performance report");
            throw;
        }
    }

    /// <summary>
    /// Exports a report in the specified format.
    /// </summary>
    /// <param name="reportType">The type of report to export.</param>
    /// <param name="startDate">The start date of the report period.</param>
    /// <param name="endDate">The end date of the report period.</param>
    /// <param name="format">The export format.</param>
    /// <returns>A task representing the asynchronous operation, containing the exportable report.</returns>
    public async Task<ExportableReport> ExportReportAsync(ReportType reportType, DateTime startDate, DateTime endDate, ExportFormat format)
    {
        DateRangeValidator.Validate(startDate, endDate);

        try
        {
            _logger.LogInformation("Exporting {ReportType} report in {Format} format for period {StartDate} to {EndDate}",
                reportType, format, startDate, endDate);

            // Generate the appropriate report based on type
            object reportData = reportType switch
            {
                ReportType.Dashboard => await GenerateDashboardReportAsync(startDate, endDate),
                ReportType.UserEngagement => await GenerateUserEngagementReportAsync(startDate, endDate),
                ReportType.ContentPerformance => await GenerateContentPerformanceReportAsync(startDate, endDate),
                ReportType.TechnicalPerformance => await GenerateTechnicalPerformanceReportAsync(startDate, endDate),
                _ => throw new ArgumentException($"Invalid report type: {reportType}", nameof(reportType))
            };

            var exportableReport = new ExportableReport
            {
                ReportType = reportType,
                Format = format,
                GeneratedAt = DateTime.UtcNow,
                Data = reportData,
                Metadata = new ReportMetadata
                {
                    Title = $"{reportType} Report",
                    Description = $"Analytics report for period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    GeneratedBy = nameof(AnalyticsReportingService),
                    Version = AnalyticsConstants.ReportVersion
                }
            };

            _logger.LogInformation("Report exported successfully");
            return exportableReport;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            throw;
        }
    }
}
