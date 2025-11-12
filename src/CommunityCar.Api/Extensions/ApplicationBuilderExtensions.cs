using CommunityCar.Api.Hubs;
using CommunityCar.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommunityCar.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application pipeline with standard middleware
    /// </summary>
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app, IHostEnvironment env)
    {
        // Request localization (should be early)
        app.UseRequestLocalization();

        // Security headers (should be first)
        app.UseSecurityHeaders();

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Static files
        app.UseStaticFiles();

        // CORS
        app.UseCors("AllowSpecificOrigins");

        // Response compression
        app.UseResponseCompression();

        // Custom middleware pipeline
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UsePerformanceMonitoring();
        app.UseRequestLogging();
        app.UseVisitorTracking();

        // Rate limiting
        app.UseRateLimiting();

        // Response caching
        app.UseResponseCaching();

        // Rate limiting (if configured)
        // app.UseIpRateLimiting();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration.TotalMilliseconds
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };
                await context.Response.WriteAsJsonAsync(result);
            }
        });

        return app;
    }

    /// <summary>
    /// Configures Swagger UI for development environment
    /// </summary>
    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Mvc.ApiVersioning.IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }

                options.RoutePrefix = string.Empty;
                options.DocumentTitle = "CommunityCar API Documentation";
                options.DefaultModelsExpandDepth(-1);
                options.DefaultModelRendering(Microsoft.AspNetCore.SwaggerUI.ModelRendering.Model);
                options.DisplayRequestDuration();
                options.EnableTryItOutByDefault();
                options.EnableDeepLinking();
                options.ShowExtensions();
            });

            // Redirect root to Swagger UI
            app.MapWhen(context => context.Request.Path == "/", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    context.Response.Redirect("/swagger");
                    await Task.CompletedTask;
                });
            });
        }

        return app;
    }

    /// <summary>
    /// Maps SignalR hubs and other endpoints
    /// </summary>
    public static IApplicationBuilder UseCustomEndpoints(this IApplicationBuilder app)
    {
        app.MapControllers();

        // Map SignalR hubs
        app.MapHub<NotificationHub>("/notificationHub");
        app.MapHub<ChatHub>("/chatHub");
        app.MapHub<ForumHub>("/forumHub");

        // Map health check endpoint (detailed)
        app.MapHealthChecks("/health/detailed");

        // Map API info endpoint
        app.MapGet("/api/info", async context =>
        {
            var info = new
            {
                name = "CommunityCar API",
                version = "1.0.0",
                environment = context.RequestServices.GetRequiredService<IHostEnvironment>().EnvironmentName,
                timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsJsonAsync(info);
        });

        return app;
    }

    /// <summary>
    /// Adds security headers middleware
    /// </summary>
    private static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Security headers
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            // Remove server header
            context.Response.Headers.Remove("Server");

            await next();
        });

        return app;
    }

    /// <summary>
    /// Configures development-specific middleware
    /// </summary>
    public static IApplicationBuilder UseDevelopmentMiddleware(this IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            // Add request logging for development
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);

                await next();

                logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
            });
        }

        return app;
    }

    /// <summary>
    /// Configures production-specific middleware
    /// </summary>
    public static IApplicationBuilder UseProductionMiddleware(this IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsProduction())
        {
            // Add HSTS
            app.UseHsts();

            // Add HTTPS redirection
            app.UseHttpsRedirection();

            // Add response caching
            app.UseResponseCaching();
        }

        return app;
    }

    /// <summary>
    /// Ensures the application is properly configured for the environment
    /// </summary>
    public static IApplicationBuilder UseEnvironmentConfiguration(this IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseDevelopmentMiddleware(env);
        app.UseProductionMiddleware(env);

        return app;
    }
}
