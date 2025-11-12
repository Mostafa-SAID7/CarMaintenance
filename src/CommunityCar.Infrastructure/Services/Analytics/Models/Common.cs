using System;

namespace CommunityCar.Infrastructure.Services.Analytics.Models;

/// <summary>
/// Represents a date range for reports.
/// </summary>
public class DateRange
{
    /// <summary>
    /// Gets or sets the start date of the range.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the end date of the range.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// Gets the duration of the date range.
    /// </summary>
    public TimeSpan Duration => End - Start;
}

/// <summary>
/// Contains metadata for exported reports.
/// </summary>
public class ReportMetadata
{
    /// <summary>
    /// Gets or sets the title of the report.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the report.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the service that generated the report.
    /// </summary>
    public string GeneratedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the report format.
    /// </summary>
    public string Version { get; set; } = AnalyticsConstants.ReportVersion;
}

/// <summary>
/// Represents an exportable report.
/// </summary>
public class ExportableReport
{
    /// <summary>
    /// Gets or sets the type of the report.
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// Gets or sets the export format.
    /// </summary>
    public ExportFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the report was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the report data.
    /// </summary>
    public object Data { get; set; } = new object();

    /// <summary>
    /// Gets or sets the metadata for the report.
    /// </summary>
    public ReportMetadata Metadata { get; set; } = new();
}
