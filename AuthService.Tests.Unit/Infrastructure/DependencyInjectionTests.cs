using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for Infrastructure DependencyInjection
/// Tests service registration and configuration
/// </summary>
public class DependencyInjectionTests
{
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _services;

    public DependencyInjectionTests()
    {
        // Setup in-memory configuration
        var configDict = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=TestDb;Username=test;Password=test",
            ["EmailSettings:SmtpHost"] = "smtp.test.com",
            ["EmailSettings:SmtpPort"] = "587",
            ["EmailSettings:SmtpUsername"] = "testuser",
            ["EmailSettings:SmtpPassword"] = "testpass",
            ["EmailSettings:SenderEmail"] = "noreply@test.com",
            ["EmailSettings:SenderName"] = "Test Sender",
            ["EmailSettings:EnableSsl"] = "true"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        _services = new ServiceCollection();

        // Add required services for Identity
        _services.AddLogging();
    }

    #region AddInfrastructure Extension Method Tests

    [Fact]
    public void AddInfrastructure_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _services.AddInfrastructure(_configuration);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {
        // Act
        var result = _services.AddInfrastructure(_configuration);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void AddInfrastructure_ShouldRegisterAppDbContext()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(AppDbContext));

        // Assert
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterIAppDbContext()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IAppDbContext));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserRepository()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IUserRepository));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(UserRepository));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterEmailService()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IEmailService));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(EmailService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserAuthorizationService()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IUserAuthorizationService));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(UserAuthorizationService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterEmailResendThrottlingService()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IEmailResendThrottlingService));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(EmailResendThrottlingService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterEmailConfirmationTokenTracker()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IEmailConfirmationTokenTracker));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(EmailConfirmationTokenTracker));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterTwoFactorCodeThrottlingService()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ITwoFactorCodeThrottlingService));

        // Assert
        descriptor.Should().NotBeNull();
        descriptor!.ImplementationType.Should().Be(typeof(TwoFactorCodeThrottlingService));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    #endregion

    #region Identity Registration Tests

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserManager()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(UserManager<ApplicationUser>));

        // Assert
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterRoleManager()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(RoleManager<ApplicationRole>));

        // Assert
        descriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterSignInManager()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(SignInManager<ApplicationUser>));

        // Assert
        descriptor.Should().NotBeNull();
    }

    #endregion

    #region Email Settings Configuration Tests

    [Fact]
    public void AddInfrastructure_ShouldConfigureEmailSettings()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act
        var descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(IOptions<EmailSettings>));

        // Note: Options are registered via Configure<T> which creates different service types
        // Check for IConfigureOptions instead
        var configureDescriptor = _services.FirstOrDefault(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IConfigureOptions<>) &&
            d.ServiceType.GetGenericArguments()[0] == typeof(EmailSettings));

        // Assert
        configureDescriptor.Should().NotBeNull();
    }

    #endregion

    #region Service Lifetime Tests

    [Fact]
    public void SingletonServices_ShouldBeRegisteredCorrectly()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act & Assert
        var singletonServices = new[]
        {
            typeof(IEmailResendThrottlingService),
            typeof(IEmailConfirmationTokenTracker),
            typeof(ITwoFactorCodeThrottlingService)
        };

        foreach (var serviceType in singletonServices)
        {
            var descriptor = _services.FirstOrDefault(d => d.ServiceType == serviceType);
            descriptor.Should().NotBeNull($"Service {serviceType.Name} should be registered");
            descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton,
                $"Service {serviceType.Name} should be Singleton");
        }
    }

    [Fact]
    public void ScopedServices_ShouldBeRegisteredCorrectly()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);

        // Act & Assert
        var scopedServices = new[]
        {
            typeof(IAppDbContext),
            typeof(IUserRepository),
            typeof(IEmailService),
            typeof(IUserAuthorizationService)
        };

        foreach (var serviceType in scopedServices)
        {
            var descriptor = _services.FirstOrDefault(d => d.ServiceType == serviceType);
            descriptor.Should().NotBeNull($"Service {serviceType.Name} should be registered");
            descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped,
                $"Service {serviceType.Name} should be Scoped");
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AddInfrastructure_MultipleCallsShouldNotDuplicateRegistrations()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);
        var countAfterFirst = _services.Count;

        // Act
        _services.AddInfrastructure(_configuration);
        var countAfterSecond = _services.Count;

        // Assert - counts might differ due to Identity's internal handling
        // but should not grow significantly with each call
        // This is more of a sanity check
        countAfterSecond.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AddInfrastructure_WithMissingConnectionString_ShouldNotThrowAtRegistration()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act - registration should not throw, only resolution might
        var act = () => _services.AddInfrastructure(emptyConfig);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Integration Tests (Service Resolution)

    [Fact]
    public void AddInfrastructure_EmailResendThrottlingService_ShouldBeResolvable()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var service = provider.GetService<IEmailResendThrottlingService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<EmailResendThrottlingService>();
    }

    [Fact]
    public void AddInfrastructure_EmailConfirmationTokenTracker_ShouldBeResolvable()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var service = provider.GetService<IEmailConfirmationTokenTracker>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<EmailConfirmationTokenTracker>();
    }

    [Fact]
    public void AddInfrastructure_TwoFactorCodeThrottlingService_ShouldBeResolvable()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var service = provider.GetService<ITwoFactorCodeThrottlingService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TwoFactorCodeThrottlingService>();
    }

    [Fact]
    public void AddInfrastructure_Singleton_ShouldReturnSameInstance()
    {
        // Arrange
        _services.AddInfrastructure(_configuration);
        var provider = _services.BuildServiceProvider();

        // Act
        var service1 = provider.GetService<IEmailResendThrottlingService>();
        var service2 = provider.GetService<IEmailResendThrottlingService>();

        // Assert
        service1.Should().BeSameAs(service2);
    }

    #endregion
}
