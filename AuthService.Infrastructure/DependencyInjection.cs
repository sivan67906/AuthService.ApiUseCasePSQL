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
        //services
        //    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
        //    {
        //        options.SignIn.RequireConfirmedEmail = true;
        //        options.Lockout.AllowedForNewUsers = true;
        //        options.Lockout.MaxFailedAccessAttempts = 5;
        //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        //        options.User.RequireUniqueEmail = true;
        //    })
        //    .AddEntityFrameworkStores<AppDbContext>()
        //    .AddDefaultTokenProviders();
        services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        //  Sign-in settings
        options.SignIn.RequireConfirmedEmail = true;

        //  Lockout settings
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        //  User settings
        options.User.RequireUniqueEmail = true;

        //  Password settings (complexity rules)
        options.Password.RequireDigit = true;              // must contain at least one digit
        options.Password.RequireLowercase = true;          // must contain at least one lowercase letter
        options.Password.RequireUppercase = true;          // must contain at least one uppercase letter
        options.Password.RequireNonAlphanumeric = true;    // must contain at least one special character
        options.Password.RequiredLength = 6;               // minimum length
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
        // Email settings configuration
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
        
        // Add Email Resend Throttling Service (Issue #2)
        services.AddSingleton<IEmailResendThrottlingService, EmailResendThrottlingService>();
        
        // Add Email Confirmation Token Tracker (Issue #4)
        services.AddSingleton<IEmailConfirmationTokenTracker, EmailConfirmationTokenTracker>();
        
        // Add 2FA Code Throttling Service (Issue #5)
        services.AddSingleton<ITwoFactorCodeThrottlingService, TwoFactorCodeThrottlingService>();
        
        return services;
    }
}
