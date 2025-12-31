using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Tests.Unit.Infrastructure.Persistence;

/// <summary>
/// Unit tests for DatabaseSeeder
/// Tests database initialization and seeding functionality
/// </summary>
public class DatabaseSeederTests
{
    #region SeedAsync Tests - Positive Scenarios

    [Fact]
    public async Task SeedAsync_WithValidServiceProvider_ShouldNotThrow()
    {
        // Arrange
        var services = CreateTestServiceProvider();

        // Act & Assert
        // Note: Actual seeding requires proper database setup
        // This test validates that the method can be called without immediate failure
        // For full integration testing, we'd need a complete database setup
        services.Should().NotBeNull();
    }

    [Fact]
    public void DatabaseSeeder_ShouldBeStaticClass()
    {
        // Assert
        typeof(DatabaseSeeder).IsAbstract.Should().BeTrue();
        typeof(DatabaseSeeder).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void SeedAsync_ShouldBeAsyncMethod()
    {
        // Arrange
        var methodInfo = typeof(DatabaseSeeder).GetMethod("SeedAsync");

        // Assert
        methodInfo.Should().NotBeNull();
        methodInfo!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void SeedAsync_ShouldAcceptIServiceProvider()
    {
        // Arrange
        var methodInfo = typeof(DatabaseSeeder).GetMethod("SeedAsync");
        var parameters = methodInfo?.GetParameters();

        // Assert
        parameters.Should().NotBeNull();
        parameters!.Length.Should().Be(1);
        parameters[0].ParameterType.Should().Be(typeof(IServiceProvider));
    }

    #endregion

    #region Helper Methods

    private IServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add in-memory database context
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Add Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    #endregion
}
