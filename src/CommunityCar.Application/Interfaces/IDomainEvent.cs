namespace CommunityCar.Application.Interfaces;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
    int EventVersion { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public int EventVersion { get; protected set; } = 1;

    protected DomainEvent()
    {
    }

    protected DomainEvent(int eventVersion)
    {
        EventVersion = eventVersion;
    }
}

public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

public interface IDomainEventSubscriber
{
    Task SubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    Task UnsubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            try
            {
                _logger.LogInformation("Handling domain event {EventType} with {HandlerType}",
                    domainEvent.EventType, handler.GetType().Name);

                await handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {EventType} with {HandlerType}",
                    domainEvent.EventType, handler.GetType().Name);
                // Continue with other handlers even if one fails
            }
        }
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var publishMethod = GetType().GetMethod(nameof(PublishAsync))!
                .MakeGenericMethod(domainEvent.GetType());

            await (Task)publishMethod.Invoke(this, new object[] { domainEvent, cancellationToken })!;
        }
    }
}

// Common domain events
public class EntityCreatedEvent<TEntity> : DomainEvent
{
    public TEntity Entity { get; }

    public EntityCreatedEvent(TEntity entity)
    {
        Entity = entity;
    }
}

public class EntityUpdatedEvent<TEntity> : DomainEvent
{
    public TEntity Entity { get; }
    public TEntity? PreviousState { get; }

    public EntityUpdatedEvent(TEntity entity, TEntity? previousState = default)
    {
        Entity = entity;
        PreviousState = previousState;
    }
}

public class EntityDeletedEvent<TEntity> : DomainEvent
{
    public object EntityId { get; }
    public TEntity? Entity { get; }

    public EntityDeletedEvent(object entityId, TEntity? entity = default)
    {
        EntityId = entityId;
        Entity = entity;
    }
}

public class UserRegisteredEvent : DomainEvent
{
    public string UserId { get; }
    public string Email { get; }
    public string UserName { get; }

    public UserRegisteredEvent(string userId, string email, string userName)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
    }
}

public class UserLoggedInEvent : DomainEvent
{
    public string UserId { get; }
    public string IpAddress { get; }
    public string UserAgent { get; }

    public UserLoggedInEvent(string userId, string ipAddress, string userAgent)
    {
        UserId = userId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}

public class PostCreatedEvent : DomainEvent
{
    public string PostId { get; }
    public string AuthorId { get; }
    public string Title { get; }
    public string Content { get; }

    public PostCreatedEvent(string postId, string authorId, string title, string content)
    {
        PostId = postId;
        AuthorId = authorId;
        Title = title;
        Content = content;
    }
}

public class CommentCreatedEvent : DomainEvent
{
    public string CommentId { get; }
    public string PostId { get; }
    public string AuthorId { get; }
    public string Content { get; }

    public CommentCreatedEvent(string commentId, string postId, string authorId, string content)
    {
        CommentId = commentId;
        PostId = postId;
        AuthorId = authorId;
        Content = content;
    }
}

public class NotificationSentEvent : DomainEvent
{
    public string NotificationId { get; }
    public string UserId { get; }
    public string Type { get; }
    public string Title { get; }
    public string Message { get; }

    public NotificationSentEvent(string notificationId, string userId, string type, string title, string message)
    {
        NotificationId = notificationId;
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
    }
}