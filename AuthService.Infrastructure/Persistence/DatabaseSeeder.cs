using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            
            logger.LogInformation("Starting database migration and seeding process");
            
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
            
            // Use comprehensive seed data that matches USER_MAPPING_EXPLAINED.md structure
            await ComprehensiveSeedData.InitializeAsync(serviceProvider);
            
            logger.LogInformation("Database seeded with comprehensive data successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
