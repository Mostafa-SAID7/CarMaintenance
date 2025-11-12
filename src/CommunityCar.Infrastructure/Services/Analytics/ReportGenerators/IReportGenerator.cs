using System;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Services.Analytics.ReportGenerators;

/// <summary>
/// Interface for report generators.
/// </summary>
/// <typeparam name="TReport">The type of report to generate.</typeparam>
public interface IReportGenerator<TReport>
{
    /// <summary>
    /// Generates a report for the specified date range.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A task representing the asynchronous operation, containing the report.</returns>
    Task<TReport> GenerateAsync(DateTime startDate, DateTime endDate);
}
