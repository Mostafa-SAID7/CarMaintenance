using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Configurations.Background;

/// <summary>
/// Interface for background job queue operations.
/// </summary>
public interface IBackgroundJobQueue
{
    /// <summary>
    /// Enqueues a job into the specified queue.
    /// </summary>
    /// <param name="job">The job information to enqueue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnqueueAsync(JobInfo job);

    /// <summary>
    /// Dequeues a job from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to dequeue from.</param>
    /// <returns>A task representing the asynchronous operation, containing the dequeued job or null if the queue is empty.</returns>
    Task<JobInfo?> DequeueAsync(string queueName);

    /// <summary>
    /// Gets the length of the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>A task representing the asynchronous operation, containing the queue length.</returns>
    Task<int> GetQueueLengthAsync(string queueName);

    /// <summary>
    /// Gets the names of all available queues.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the collection of queue names.</returns>
    Task<IEnumerable<string>> GetQueueNamesAsync();

    /// <summary>
    /// Peeks at the next job in the specified queue without removing it.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <returns>A task representing the asynchronous operation, containing the next job or null if the queue is empty.</returns>
    Task<JobInfo?> PeekAsync(string queueName);

    /// <summary>
    /// Clears all jobs from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to clear.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearQueueAsync(string queueName);
}
