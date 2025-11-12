using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Configurations.Background;

/// <summary>
/// In-memory implementation of the background job queue.
/// This implementation uses concurrent collections for thread-safe operations.
/// </summary>
public class InMemoryJobQueue : IBackgroundJobQueue
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<JobInfo>> _queues = new();
    private readonly BackgroundJobSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryJobQueue"/> class.
    /// </summary>
    /// <param name="settings">The background job settings.</param>
    public InMemoryJobQueue(BackgroundJobSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Enqueues a job into the specified queue.
    /// </summary>
    /// <param name="job">The job information to enqueue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue exceeds the maximum allowed jobs.</exception>
    public async Task EnqueueAsync(JobInfo job)
    {
        var queue = _queues.GetOrAdd(job.Queue, _ => new ConcurrentQueue<JobInfo>());

        // Check queue length limit
        if (queue.Count >= _settings.MaxJobsPerQueue)
        {
            throw new InvalidOperationException($"Queue '{job.Queue}' has reached the maximum capacity of {_settings.MaxJobsPerQueue} jobs.");
        }

        queue.Enqueue(job);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Dequeues a job from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to dequeue from.</param>
    /// <returns>A task representing the asynchronous operation, containing the dequeued job or null if the queue is empty.</returns>
    public async Task<JobInfo?> DequeueAsync(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue) && queue.TryDequeue(out var job))
        {
            return await Task.FromResult(job);
        }
        return await Task.FromResult<JobInfo?>(null);
    }

    /// <summary>
    /// Gets the length of the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>A task representing the asynchronous operation, containing the queue length.</returns>
    public async Task<int> GetQueueLengthAsync(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            return await Task.FromResult(queue.Count);
        }
        return await Task.FromResult(0);
    }

    /// <summary>
    /// Gets the names of all available queues.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the collection of queue names.</returns>
    public async Task<IEnumerable<string>> GetQueueNamesAsync()
    {
        return await Task.FromResult(_queues.Keys.AsEnumerable());
    }

    /// <summary>
    /// Peeks at the next job in the specified queue without removing it.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>A task representing the asynchronous operation, containing the next job or null if the queue is empty.</returns>
    public async Task<JobInfo?> PeekAsync(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue) && queue.TryPeek(out var job))
        {
            return await Task.FromResult(job);
        }
        return await Task.FromResult<JobInfo?>(null);
    }

    /// <summary>
    /// Clears all jobs from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to clear.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ClearQueueAsync(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            while (queue.TryDequeue(out _)) { }
        }
        await Task.CompletedTask;
    }
}
