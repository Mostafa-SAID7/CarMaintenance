e using CommunityCar.Application.Interfaces.Commuinty;
using CommunityCar.Application.Interfaces.Hub;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityCar.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register repositories and Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        // Register application services implemented in infrastructure
        services.AddScoped<INotificationService, Services.NotificationService>();
        services.AddScoped<IChatService, Services.ChatService>();
        services.AddScoped<IPostService, Services.PostService>();
        services.AddScoped<ICommentService, Services.CommentService>();

        // Register specific repositories if needed
        // services.AddScoped<ICarRepository, CarRepository>(); // Commented out - repository not implemented
        // services.AddScoped<IBookingRepository, BookingRepository>(); // Commented out - repository not implemented
        // services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // Commented out - repository not implemented

        return services;
    }
}