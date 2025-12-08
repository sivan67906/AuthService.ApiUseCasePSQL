using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // ============================================
        // OPTIMIZATION: PostgreSQL DbContext with Connection Pooling
        // ============================================
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                // Connection Pooling Settings
                npgsqlOptions.MaxBatchSize(100); // Batch multiple commands together
                npgsqlOptions.CommandTimeout(30); // 30 seconds timeout
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null
                );
            });

            // OPTIMIZATION: Disable tracking for all queries by default (better for read-heavy operations)
            // Individual commands can still use .AsTracking() when needed for updates
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // OPTIMIZATION: Enable sensitive data logging only in development
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // Register single unified DbContext interface
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        // Email settings configuration
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
        return services;
    }
}
