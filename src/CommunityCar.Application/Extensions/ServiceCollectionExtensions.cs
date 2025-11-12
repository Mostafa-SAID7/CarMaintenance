using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CommunityCar.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        // services.AddScoped<IAuthService, AuthService>(); // Commented out - service not implemented
        // services.AddScoped<ITokenService, TokenService>(); // Commented out - service not implemented
        // services.AddScoped<IOtpService, OtpService>(); // Commented out - interface not found
        // services.AddScoped<IEmailService, Services.EmailService>(); // Commented out - service not implemented
        // services.AddScoped<IProfileService, ProfileService>(); // Commented out - service not implemented
        // services.AddScoped<ICarService, CarService>(); // Commented out - service not implemented
        // services.AddScoped<IBookingService, BookingService>(); // Commented out - service not implemented
        // services.AddScoped<IAuditService, AuditService>(); // Commented out - service not implemented

        // Register FluentValidation
        services.AddFluentValidationAutoValidation();
        // services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>(); // Commented out - validator not implemented
        // services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>(); // Commented out - validator not implemented
        // services.AddValidatorsFromAssemblyContaining<UpdateProfileRequestValidator>(); // Commented out - validator not implemented
        // services.AddValidatorsFromAssemblyContaining<global::CommunityCar.Application.Validators.Notifications.CreateNotificationRequestValidator>(); // Commented out - validator not implemented
        // services.AddValidatorsFromAssemblyContaining<global::CommunityCar.Application.Validators.Chat.SendChatMessageRequestValidator>(); // Commented out - validator not implemented
        // services.AddValidatorsFromAssemblyContaining<CreateCarRequestValidator>(); // Commented out - validator not implemented
        // services.AddValidatorsFromAssemblyContaining<CreateBookingRequestValidator>(); // Commented out - validator not implemented

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        // Register AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions));

        return services;
    }


    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Register API-specific services
        // services.AddScoped<ICurrentUserService, CurrentUserService>(); // Commented out - service not implemented
        // services.AddScoped<IApiResponseService, ApiResponseService>(); // Commented out - service not implemented

        // Register background services
        // services.AddHostedService<EmailBackgroundService>(); // Commented out - service not implemented
        // services.AddHostedService<AuditCleanupService>(); // Commented out - service not implemented
        // services.AddHostedService<ExpiredBookingCleanupService>(); // Commented out - service not implemented

        return services;
    }
}