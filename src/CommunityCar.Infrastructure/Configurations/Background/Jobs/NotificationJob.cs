using CommunityCar.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityCar.Infrastructure.Configurations.Background.Jobs;

/// <summary>
/// Job for sending notifications.
/// </summary>
public class NotificationJob : IJob<NotificationJobArgs>
{
    /// <summary>
    /// Executes the notification job.
    /// </summary>
    /// <param name="args">The job arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(NotificationJobArgs args, CancellationToken cancellationToken = default)
    {
        // Validate arguments
        if (args == null)
            throw new System.ArgumentNullException(nameof(args));

        if (string.IsNullOrEmpty(args.UserId))
            throw new System.ArgumentException("UserId is required", nameof(args.UserId));

        if (string.IsNullOrEmpty(args.Message))
            throw new System.ArgumentException("Message is required", nameof(args.Message));

        // Simulate notification sending with cancellation support
        await Task.Delay(100, cancellationToken);

        // In a real implementation, this would send the notification via email, push, etc.
        // Example: await _notificationService.SendAsync(args.UserId, args.Message, args.Type, cancellationToken);
    }
}

/// <summary>
/// Arguments for the notification job.
/// </summary>
public class NotificationJobArgs
{
    /// <summary>
    /// Gets or sets the user ID to send the notification to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
