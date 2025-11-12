using CommunityCar.Application.Interfaces.Hub;
using CommunityCar.Domain.Entities.Chat;
using CommunityCar.Domain.Entities.Notifications;
using CommunityCar.Infrastructure.Services.Communication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CommunityCar.Infrastructure.Configurations.Communication;

public static class CommunicationConfiguration
{
    public static IServiceCollection AddCommunicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register communication services
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Configure SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100KB
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        });

        // Configure communication settings
        services.Configure<CommunicationSettings>(configuration.GetSection("Communication"));

        // Register repositories for communication
        services.AddScoped<IRepository<ChatMessage>, BaseRepository<ChatMessage>>();
        services.AddScoped<IRepository<Notification>, BaseRepository<Notification>>();

        // Register real-time connection tracking
        services.AddSingleton<IConnectionTracker, ConnectionTracker>();

        return services;
    }
}

public class CommunicationSettings
{
    public bool EnableRealTimeNotifications { get; set; } = true;
    public int MaxConnectionsPerUser { get; set; } = 5;
    public int MessageRetentionDays { get; set; } = 30;
    public int MaxMessageLength { get; set; } = 2000;
    public bool EnableMessageEncryption { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool EnableTypingIndicators { get; set; } = true;
    public bool EnableReadReceipts { get; set; } = true;
    public int MaxGroupSize { get; set; } = 100;
    public bool EnableFileSharing { get; set; } = true;
    public long MaxFileSizeBytes { get; set; } = 10485760; // 10MB
}

public interface IConnectionTracker
{
    Task AddConnectionAsync(string userId, string connectionId);
    Task RemoveConnectionAsync(string connectionId);
    Task<IEnumerable<string>> GetConnectionsAsync(string userId);
    Task<bool> IsUserOnlineAsync(string userId);
    Task<int> GetOnlineUserCountAsync();
}

public class ConnectionTracker : IConnectionTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private readonly ConcurrentDictionary<string, string> _connectionToUser = new();

    public async Task AddConnectionAsync(string userId, string connectionId)
    {
        var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
        connections.Add(connectionId);
        _connectionToUser[connectionId] = userId;
    }

    public async Task RemoveConnectionAsync(string connectionId)
    {
        if (_connectionToUser.TryRemove(connectionId, out var userId))
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }
    }

    public async Task<IEnumerable<string>> GetConnectionsAsync(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connections)
            ? connections.ToList()
            : new List<string>();
    }

    public async Task<bool> IsUserOnlineAsync(string userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    public async Task<int> GetOnlineUserCountAsync()
    {
        return _userConnections.Count;
    }
}

// Message broker for cross-instance communication
public interface IMessageBroker
{
    Task PublishAsync<T>(string channel, T message);
    Task SubscribeAsync<T>(string channel, Func<T, Task> handler);
    Task UnsubscribeAsync(string channel);
}

public class InMemoryMessageBroker : IMessageBroker
{
    private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _subscribers = new();

    public async Task PublishAsync<T>(string channel, T message)
    {
        if (_subscribers.TryGetValue(channel, out var handlers))
        {
            var tasks = handlers.Select(handler => handler(message!));
            await Task.WhenAll(tasks);
        }
    }

    public async Task SubscribeAsync<T>(string channel, Func<T, Task> handler)
    {
        var subscribers = _subscribers.GetOrAdd(channel, _ => new List<Func<object, Task>>());
        subscribers.Add(async obj => await handler((T)obj));
    }

    public async Task UnsubscribeAsync(string channel)
    {
        _subscribers.TryRemove(channel, out _);
    }
}

// Notification templates
public interface INotificationTemplateService
{
    Task<string> RenderTemplateAsync(string templateName, object model);
    Task RegisterTemplateAsync(string name, string template);
}

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly ConcurrentDictionary<string, string> _templates = new();
    private readonly IRazorViewEngine _razorViewEngine;

    public NotificationTemplateService(IRazorViewEngine razorViewEngine)
    {
        _razorViewEngine = razorViewEngine;

        // Register default templates
        RegisterDefaultTemplates();
    }

    public async Task<string> RenderTemplateAsync(string templateName, object model)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }

        // In a real implementation, use Razor to render the template
        // For now, return a simple formatted string
        return template.Replace("{{Model}}", model.ToString() ?? string.Empty);
    }

    public async Task RegisterTemplateAsync(string name, string template)
    {
        _templates[name] = template;
    }

    private void RegisterDefaultTemplates()
    {
        _templates["welcome_email"] = "Welcome {{Model}} to our community!";
        _templates["password_reset"] = "Click here to reset your password: {{Model}}";
        _templates["new_message"] = "You have a new message from {{Model}}";
        _templates["post_liked"] = "Your post was liked by {{Model}}";
    }
}
