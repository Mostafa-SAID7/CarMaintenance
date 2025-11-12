using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Configurations.Background.Jobs;

/// <summary>
/// Job for processing analytics data.
/// This job retrieves and processes analytics metrics for a specified data type and date range.
/// </summary>
public sealed class AnalyticsJob : IJob<AnalyticsJobArgs>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsJob"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service to retrieve data.</param>
    /// <param name="logger">The logger for tracking job execution.</param>
    public AnalyticsJob(IAnalyticsService analyticsService, ILogger<AnalyticsJob> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the analytics job asynchronously.
    /// </summary>
    /// <param name="args">The job arguments containing data type and date range.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when args is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public async Task ExecuteAsync(AnalyticsJobArgs args, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analytics job for data type '{DataType}' from {StartDate} to {EndDate}",
            args.DataType, args.StartDate, args.EndDate);

        ValidateArgs(args);

        try
        {
            // Retrieve analytics metrics for the specified period
            var metrics = await _analyticsService.GetMetricsAsync(args.StartDate, args.EndDate, cancellationToken);

            _logger.LogInformation("Retrieved analytics metrics: TotalEvents={TotalEvents}, UniqueUsers={UniqueUsers}, PageViews={PageViews}",
                metrics.TotalEvents, metrics.UniqueUsers, metrics.PageViews);

            // Additional processing could be added here, such as:
            // - Storing metrics in a database
            // - Generating reports
            // - Triggering alerts based on thresholds
            // - Aggregating data for dashboards

            _logger.LogInformation("Analytics job completed successfully for data type '{DataType}'", args.DataType);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Analytics job was cancelled for data type '{DataType}'", args.DataType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing analytics job for data type '{DataType}'", args.DataType);
            throw;
        }
    }

    /// <summary>
    /// Validates the job arguments.
    /// </summary>
    /// <param name="args">The arguments to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when args is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateArgs(AnalyticsJobArgs args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (string.IsNullOrWhiteSpace(args.DataType))
            throw new ArgumentException("DataType cannot be null, empty, or whitespace", nameof(args.DataType));

        var now = DateTime.UtcNow;
        var minDate = new DateTime(2020, 1, 1); // Reasonable minimum date

        if (args.StartDate < minDate)
            throw new ArgumentException($"StartDate cannot be before {minDate:yyyy-MM-dd}", nameof(args.StartDate));

        if (args.EndDate > now.AddDays(1)) // Allow up to tomorrow for processing delays
            throw new ArgumentException("EndDate cannot be in the future", nameof(args.EndDate));

        if (args.StartDate >= args.EndDate)
            throw new ArgumentException("StartDate must be before EndDate", nameof(args.StartDate));

        var dateRange = args.EndDate - args.StartDate;
        if (dateRange > TimeSpan.FromDays(365)) // Limit to one year for performance
            throw new ArgumentException("Date range cannot exceed 365 days", nameof(args.EndDate));
    }
}

/// <summary>
/// Arguments for the analytics job.
/// </summary>
/// <param name="DataType">The type of data to process (e.g., "user", "content").</param>
/// <param name="StartDate">The start date for the analytics period (UTC).</param>
/// <param name="EndDate">The end date for the analytics period (UTC).</param>
public record AnalyticsJobArgs(
    string DataType,
    DateTime StartDate,
    DateTime EndDate
);
