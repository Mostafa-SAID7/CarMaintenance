using CommunityCar.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Configurations.Background;

/// <summary>
/// Background service that processes queued jobs.
/// </summary>
public class BackgroundJobHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BackgroundJobSettings _settings;
    private readonly ILogger<BackgroundJobHostedService> _logger;
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundJobHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="settings">The background job settings.</param>
    /// <param name="logger">The logger instance.</param>
    public BackgroundJobHostedService(
        IServiceProvider serviceProvider,
        IOptions<BackgroundJobSettings> settings,
        ILogger<BackgroundJobHostedService> _logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        this._logger = _logger;
        _semaphore = new SemaphoreSlim(_settings.MaxConcurrentJobs);
    }

    /// <summary>
    /// Executes the background job processing loop.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job processing started with max concurrent jobs: {MaxConcurrentJobs}",
            _settings.MaxConcurrentJobs);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in background job processing loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.QueueProcessingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Background job processing stopped");
    }

    private async Task ProcessJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var queue = scope.ServiceProvider.GetRequiredService<IBackgroundJobQueue>();
        var queueNames = await queue.GetQueueNamesAsync();

        // Limit the number of jobs processed per cycle to prevent long-running loops
        const int maxJobsPerCycle = 10;
        int jobsProcessed = 0;

        foreach (var queueName in queueNames)
        {
            if (jobsProcessed >= maxJobsPerCycle || stoppingToken.IsCancellationRequested)
                break;

            var job = await queue.DequeueAsync(queueName);
            if (job != null)
            {
                await _semaphore.WaitAsync(stoppingToken);

                // Fire-and-forget with proper error handling
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessJobAsync(job, scope.ServiceProvider, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing job {JobId}", job.Id);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, stoppingToken);

                jobsProcessed++;
            }
        }
    }

    private async Task ProcessJobAsync(JobInfo job, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;

            _logger.LogInformation("Processing job {JobId} of type {JobType}", job.Id, job.Type);

            // Resolve and execute the job
            var jobType = Type.GetType(job.Type);
            if (jobType == null)
            {
                throw new InvalidOperationException($"Job type '{job.Type}' could not be resolved.");
            }

            var jobInstance = serviceProvider.GetService(jobType);
            if (jobInstance is not IJob jobInterface)
            {
                throw new InvalidOperationException($"Job type '{job.Type}' does not implement IJob interface.");
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                new CancellationTokenSource(TimeSpan.FromMinutes(_settings.JobTimeoutMinutes)).Token);

            await jobInterface.ExecuteAsync(job.Args, cts.Token);

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Job {JobId} completed successfully in {Duration}",
                job.Id, job.ProcessingDuration);
        }
        catch (OperationCanceledException)
        {
            job.Status = JobStatus.Cancelled;
            _logger.LogWarning("Job {JobId} was cancelled", job.Id);
        }
        catch (Exception ex)
        {
            await HandleJobFailureAsync(job, ex, serviceProvider);
        }
    }

    private async Task HandleJobFailureAsync(JobInfo job, Exception ex, IServiceProvider serviceProvider)
    {
        job.Status = JobStatus.Failed;
        job.Error = ex.Message;
        job.FailedAt = DateTime.UtcNow;

        _logger.LogError(ex, "Job {JobId} failed with error: {Error}", job.Id, ex.Message);

        // Implement exponential backoff for retries
        if (job.RetryCount < _settings.MaxRetryAttempts)
        {
            job.RetryCount++;
            job.Status = JobStatus.Queued;

            // Exponential backoff: base delay * 2^(retryCount - 1)
            var delaySeconds = _settings.RetryDelaySeconds * Math.Pow(2, job.RetryCount - 1);
            job.ScheduledAt = DateTime.UtcNow.AddSeconds(delaySeconds);

            // Re-queue the job
            var queue = serviceProvider.GetRequiredService<IBackgroundJobQueue>();
            await queue.EnqueueAsync(job);

            _logger.LogInformation("Job {JobId} scheduled for retry {RetryCount} at {ScheduledTime}",
                job.Id, job.RetryCount, job.ScheduledAt);
        }
        else
        {
            _logger.LogWarning("Job {JobId} exceeded maximum retry attempts ({MaxRetries})",
                job.Id, _settings.MaxRetryAttempts);
        }
    }
}
