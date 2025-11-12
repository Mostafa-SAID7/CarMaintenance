using System;
using System.Collections.Generic;

namespace CommunityCar.Infrastructure.Services.Analytics.Models;

/// <summary>
/// Performance metrics for the system.
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Gets or sets the average response time.
    /// </summary>
    public TimeSpan AverageResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the error rate.
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Gets or sets the uptime percentage.
    /// </summary>
    public double UptimePercentage { get; set; }

    /// <summary>
    /// Gets or sets the throughput.
    /// </summary>
    public double Throughput { get; set; }

    /// <summary>
    /// Gets or sets the peak concurrent users.
    /// </summary>
    public int PeakConcurrentUsers { get; set; }
}

/// <summary>
/// Details about an error.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Gets or sets the error type.
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of occurrences.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the number of affected users.
    /// </summary>
    public int AffectedUsers { get; set; }

    /// <summary>
    /// Gets or sets the last occurrence time.
    /// </summary>
    public DateTime LastOccurred { get; set; }
}

/// <summary>
/// Error analysis data.
/// </summary>
public class ErrorAnalysis
{
    /// <summary>
    /// Gets or sets the top errors.
    /// </summary>
    public List<ErrorDetails> TopErrors { get; set; } = new();

    /// <summary>
    /// Gets or sets the error trends.
    /// </summary>
    public Dictionary<DateTime, int> ErrorTrends { get; set; } = new();

    /// <summary>
    /// Gets or sets the error categories.
    /// </summary>
    public Dictionary<string, int> ErrorCategories { get; set; } = new();
}

/// <summary>
/// System health metrics.
/// </summary>
public class SystemHealthMetrics
{
    /// <summary>
    /// Gets or sets the CPU usage percentage.
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// Gets or sets the memory usage percentage.
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// Gets or sets the disk usage percentage.
    /// </summary>
    public double DiskUsage { get; set; }

    /// <summary>
    /// Gets or sets the number of database connections.
    /// </summary>
    public int DatabaseConnections { get; set; }

    /// <summary>
    /// Gets or sets the cache hit rate percentage.
    /// </summary>
    public double CacheHitRate { get; set; }
}

/// <summary>
/// Technical performance report.
/// </summary>
public class TechnicalPerformanceReport
{
    /// <summary>
    /// Gets or sets the report period.
    /// </summary>
    public DateRange ReportPeriod { get; set; } = new();

    /// <summary>
    /// Gets or sets the performance metrics.
    /// </summary>
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the error analysis.
    /// </summary>
    public ErrorAnalysis ErrorAnalysis { get; set; } = new();

    /// <summary>
    /// Gets or sets the system health metrics.
    /// </summary>
    public SystemHealthMetrics SystemHealth { get; set; } = new();
}
