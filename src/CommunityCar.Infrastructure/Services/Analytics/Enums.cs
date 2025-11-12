namespace CommunityCar.Infrastructure.Services.Analytics;

/// <summary>
/// Represents the type of analytics report.
/// </summary>
public enum ReportType
{
    /// <summary>
    /// Dashboard analytics report.
    /// </summary>
    Dashboard,

    /// <summary>
    /// User engagement report.
    /// </summary>
    UserEngagement,

    /// <summary>
    /// Content performance report.
    /// </summary>
    ContentPerformance,

    /// <summary>
    /// Technical performance report.
    /// </summary>
    TechnicalPerformance
}

/// <summary>
/// Represents the export format for reports.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// PDF format.
    /// </summary>
    PDF,

    /// <summary>
    /// Excel format.
    /// </summary>
    Excel,

    /// <summary>
    /// CSV format.
    /// </summary>
    CSV,

    /// <summary>
    /// JSON format.
    /// </summary>
    JSON
}
