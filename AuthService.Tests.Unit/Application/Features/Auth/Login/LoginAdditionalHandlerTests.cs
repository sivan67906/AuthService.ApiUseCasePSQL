using AuthService.Application.Features.Auth.Login;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using OtpNet;

namespace AuthService.Tests.Unit.Application.Features.Auth.Login;

#region ResendTwoFactorLoginCode Tests

public class ResendTwoFactorLoginCodeCommandHandlerTests : ApplicationTestBase
{
    private readonly ResendTwoFactorLoginCodeCommandHandler _handler;
    private readonly Mock<ILogger<ResendTwoFactorLoginCodeCommandHandler>> _loggerMock;

    public ResendTwoFactorLoginCodeCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<ResendTwoFactorLoginCodeCommandHandler>();

        _handler = new ResendTwoFactorLoginCodeCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            TwoFactorThrottlingServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ResendTwoFactorLoginCodeCommand("nonexistent@example.com", "token");

        UserManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_InvalidTwoFactorToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        user.SecurityStamp = "valid-stamp";
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", "invalid-stamp");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid or expired two-factor session*");
    }

    [Fact]
    public async Task Handle_AuthenticatorEnabled_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        user.SecurityStamp = "valid-stamp";
        user.AuthenticatorEnabled = true;
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", "valid-stamp");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Authenticator codes*");
    }

    [Fact]
    public async Task Handle_Throttled_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        user.SecurityStamp = "valid-stamp";
        user.AuthenticatorEnabled = false;
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", "valid-stamp");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((false, "Too many attempts", TimeSpan.FromSeconds(60)));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Too many attempts*");
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsResult()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        user.SecurityStamp = "valid-stamp";
        user.AuthenticatorEnabled = false;
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", "valid-stamp");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider))
            .ReturnsAsync("123456");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.NewTwoFactorToken.Should().Be("valid-stamp");
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_StoresCodeAndRecordsAttempt()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        user.SecurityStamp = "valid-stamp";
        user.AuthenticatorEnabled = false;
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", "valid-stamp");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider))
            .ReturnsAsync("123456");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        TwoFactorThrottlingServiceMock.Verify(x => x.StoreCode("test@example.com", "123456", It.IsAny<DateTime>()), Times.Once);
        TwoFactorThrottlingServiceMock.Verify(x => x.RecordResendAttempt("test@example.com"), Times.Once);
    }
}

public class ResendTwoFactorLoginCodeCommandValidatorTests
{
    private readonly ResendTwoFactorLoginCodeCommandValidator _validator;

    public ResendTwoFactorLoginCodeCommandValidatorTests()
    {
        _validator = new ResendTwoFactorLoginCodeCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", "token");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyEmail_FailsValidation(string email)
    {
        var command = new ResendTwoFactorLoginCodeCommand(email, "token");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyToken_FailsValidation(string token)
    {
        var command = new ResendTwoFactorLoginCodeCommand("test@example.com", token);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region VerifyTwoFactorLogin Tests

public class VerifyTwoFactorLoginCommandHandlerTests
{
    private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private Mock<IConfiguration> CreateConfigurationMock()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly12345678");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        return configMock;
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        var command = new VerifyTwoFactorLoginCommand("notfound@example.com", "123456", "token", "Email");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_InvalidTwoFactorToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        // Create user with specific SecurityStamp
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "invalidtoken@example.com",
            UserName = "invalidtoken@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.SecurityStamp = "correct-stamp";  // Set after construction

        var command = new VerifyTwoFactorLoginCommand("invalidtoken@example.com", "123456", "wrong-stamp", "Email");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid or expired two-factor session*");
    }

    [Fact]
    public async Task Handle_AuthenticatorNotConfigured_ThrowsInvalidOperationException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        // Use a shared stamp value
        const string stampValue = "matching-stamp-auth-notconfig";

        // Create user with matching SecurityStamp
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "authnotconfig@example.com",
            UserName = "authnotconfig@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            AuthenticatorSecretKey = null
        };
        user.SecurityStamp = stampValue;  // Set after construction

        var command = new VerifyTwoFactorLoginCommand("authnotconfig@example.com", "123456", stampValue, "Authenticator");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Authenticator not configured.");
    }

    [Fact]
    public async Task Handle_InvalidAuthenticatorCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        const string stampValue = "matching-stamp-invalid-auth-code";
        var secretKey = KeyGeneration.GenerateRandomKey(20);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "invalidauthcode@example.com",
            UserName = "invalidauthcode@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            AuthenticatorSecretKey = Base32Encoding.ToString(secretKey)
        };
        user.SecurityStamp = stampValue;

        var command = new VerifyTwoFactorLoginCommand("invalidauthcode@example.com", "000000", stampValue, "Authenticator");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid verification code.");
    }

    [Fact]
    public async Task Handle_ValidAuthenticatorCode_ReturnsTokens()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        const string stampValue = "matching-stamp-valid-auth-code";
        var secretKey = KeyGeneration.GenerateRandomKey(20);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "validauthcode@example.com",
            UserName = "validauthcode@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            AuthenticatorSecretKey = Base32Encoding.ToString(secretKey)
        };
        user.SecurityStamp = stampValue;

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var command = new VerifyTwoFactorLoginCommand("validauthcode@example.com", validCode, stampValue, "Authenticator");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });
        userManagerMock.Setup(x => x.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RequiresTwoFactor.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidEmailCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        const string stampValue = "matching-stamp-invalid-email-code";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "invalidemailcode@example.com",
            UserName = "invalidemailcode@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.SecurityStamp = stampValue;

        var command = new VerifyTwoFactorLoginCommand("invalidemailcode@example.com", "000000", stampValue, "Email");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        throttlingMock.Setup(x => x.ValidateCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid verification code.");
    }

    [Fact]
    public async Task Handle_ValidEmailCode_ReturnsTokens()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        const string stampValue = "matching-stamp-valid-email-code";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "validemailcode@example.com",
            UserName = "validemailcode@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.SecurityStamp = stampValue;

        var command = new VerifyTwoFactorLoginCommand("validemailcode@example.com", "123456", stampValue, "Email");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        throttlingMock.Setup(x => x.ValidateCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(It.IsAny<ApplicationUser>(), TokenOptions.DefaultEmailProvider, It.IsAny<string>()))
            .ReturnsAsync(true);
        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });
        userManagerMock.Setup(x => x.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RequiresTwoFactor.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidCode_ClearsAttemptsAndUpdatesStamp()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var configMock = CreateConfigurationMock();
        var dbContextMock = new Mock<IAppDbContext>();
        var throttlingMock = new Mock<ITwoFactorCodeThrottlingService>();

        var handler = new VerifyTwoFactorLoginCommandHandler(
            userManagerMock.Object, configMock.Object, dbContextMock.Object, throttlingMock.Object);

        const string stampValue = "matching-stamp-clear-attempts";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "clearattempts@example.com",
            UserName = "clearattempts@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.SecurityStamp = stampValue;

        var command = new VerifyTwoFactorLoginCommand("clearattempts@example.com", "123456", stampValue, "Email");
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        throttlingMock.Setup(x => x.ValidateCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(It.IsAny<ApplicationUser>(), TokenOptions.DefaultEmailProvider, It.IsAny<string>()))
            .ReturnsAsync(true);
        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });
        userManagerMock.Setup(x => x.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        throttlingMock.Verify(x => x.ClearAttempts(It.IsAny<string>()), Times.Once);
        userManagerMock.Verify(x => x.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }
}

#endregion
