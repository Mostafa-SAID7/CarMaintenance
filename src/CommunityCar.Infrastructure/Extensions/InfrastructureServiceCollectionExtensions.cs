using CommunityCar.Application.Interfaces;
using CommunityCar.Application.Interfaces.Auth;
using CommunityCar.Application.Interfaces.Community;
using CommunityCar.Application.Interfaces.Hub;
using CommunityCar.Application.Interfaces.Social;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Configurations.Analytics;
using CommunityCar.Infrastructure.Configurations.Auth;
using CommunityCar.Infrastructure.Configurations.Background;
using CommunityCar.Infrastructure.Configurations.Caching;
using CommunityCar.Infrastructure.Configurations.Communication;
using CommunityCar.Infrastructure.Configurations.Content;
using CommunityCar.Infrastructure.Configurations.External;
using CommunityCar.Infrastructure.Configurations.Infrastructure;
using CommunityCar.Infrastructure.Repositories;
using CommunityCar.Infrastructure.Services.Analytics;
using CommunityCar.Infrastructure.Services.Authentication;
using CommunityCar.Infrastructure.Services.Background;
using CommunityCar.Infrastructure.Services.Caching;
using CommunityCar.Infrastructure.Services.Communication;
using CommunityCar.Infrastructure.Services.Content;
using CommunityCar.Infrastructure.Services.External;
using CommunityCar.Infrastructure.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityCar.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories and Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        // Register application services implemented in infrastructure (updated paths)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();

        // Register additional services from organized folders
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IVisitorTrackingService, VisitorTrackingService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICachedProfileService, CachedProfileService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ISitemapService, SitemapService>();
        services.AddScoped<IViewRenderService, ViewRenderService>();

        // Register social media services
        services.AddScoped<IFacebookService, FacebookService>();
        services.AddScoped<IGoogleService, GoogleService>();
        services.AddScoped<ILinkedInService, LinkedInService>();
        services.AddScoped<ITwitterService, TwitterService>();

        // Register specific repositories if needed
        // services.AddScoped<ICarRepository, CarRepository>(); // Commented out - repository not implemented
        // services.AddScoped<IBookingRepository, BookingRepository>(); // Commented out - repository not implemented
        // services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // Commented out - repository not implemented

        return services;
    }

    public static IServiceCollection AddAllConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add all configuration sections
        services.AddIdentityConfiguration(configuration);
        services.AddAnalyticsConfiguration(configuration);
        services.AddBackgroundConfiguration(configuration);
        services.AddCachingConfiguration(configuration);
        services.AddCommunicationConfiguration(configuration);
        services.AddContentConfiguration(configuration);
        services.AddExternalConfiguration(configuration);
        services.AddInfrastructureConfiguration(configuration);

        return services;
    }
}
