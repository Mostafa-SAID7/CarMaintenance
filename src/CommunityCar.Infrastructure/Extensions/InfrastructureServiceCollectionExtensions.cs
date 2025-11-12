e using CommunityCar.Domain.Interfaces;
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
        services.AddScoped<CommunityCar.Application.Interfaces.INotificationService, Services.NotificationService>();
        services.AddScoped<CommunityCar.Application.Interfaces.IChatService, Services.ChatService>();
        services.AddScoped<CommunityCar.Application.Interfaces.IPostService, Services.PostService>();
        services.AddScoped<CommunityCar.Application.Interfaces.ICommentService, Services.CommentService>();

        // Register specific repositories if needed
        // services.AddScoped<ICarRepository, CarRepository>(); // Commented out - repository not implemented
        // services.AddScoped<IBookingRepository, BookingRepository>(); // Commented out - repository not implemented
        // services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // Commented out - repository not implemented

        return services;
    }
}