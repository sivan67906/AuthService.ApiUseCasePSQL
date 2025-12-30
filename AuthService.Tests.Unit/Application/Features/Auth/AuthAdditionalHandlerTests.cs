using AuthService.Application.Features.Auth.ForgotPassword;
using AuthService.Application.Features.Auth.RefreshToken;
using AuthService.Application.Features.Auth.Register;
using AuthService.Application.Features.Auth.ResetPassword;
using AuthService.Application.Features.Auth.RevokeToken;
using MockQueryable.Moq;

namespace AuthService.Tests.Unit.Application.Features.Auth;

#region RefreshToken Tests

public class RefreshTokenCommandHandlerTests : ApplicationTestBase
{
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            DbContextMock.Object,
            UserManagerMock.Object,
            ConfigurationMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var refreshToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        var tokens = new List<UserRefreshToken> { refreshToken };
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var command = new RefreshTokenCommand("valid-refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.NewRefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var tokens = new List<UserRefreshToken>();
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);

        var command = new RefreshTokenCommand("invalid-token");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid*");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var expiredToken = new UserRefreshToken
        {
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        var tokens = new List<UserRefreshToken> { expiredToken };
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);

        var command = new RefreshTokenCommand("expired-token");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsInvalidOperationException()
    {
        var revokedToken = new UserRefreshToken
        {
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = true
        };

        var tokens = new List<UserRefreshToken> { revokedToken };
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);

        var command = new RefreshTokenCommand("revoked-token");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

#endregion

#region ForgotPassword Tests

public class ForgotPasswordCommandHandlerTests : ApplicationTestBase
{
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _handler = new ForgotPasswordCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidEmail_ReturnsTrue()
    {
        var user = CreateTestUser(email: "user@example.com", emailConfirmed: true);

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ForgotPasswordCommand("user@example.com", "https://example.com/reset", "192.168.1.1");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ReturnsTrue()
    {
        UserManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new ForgotPasswordCommand("nonexistent@example.com", "https://example.com/reset", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NullIpAddress_Succeeds()
    {
        var user = CreateTestUser(email: "user@example.com", emailConfirmed: true);

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ForgotPasswordCommand("user@example.com", "https://example.com/reset", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }
}

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator;

    public ForgotPasswordCommandValidatorTests()
    {
        _validator = new ForgotPasswordCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new ForgotPasswordCommand("user@example.com", "https://example.com/reset", "192.168.1.1");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyEmail_FailsValidation(string email)
    {
        var command = new ForgotPasswordCommand(email, "https://example.com/reset", null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@domain.com")]
    public void Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        var command = new ForgotPasswordCommand(email, "https://example.com/reset", null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyCallbackUrl_FailsValidation(string url)
    {
        var command = new ForgotPasswordCommand("user@example.com", url, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region ResetPassword Tests

public class ResetPasswordCommandHandlerTests : ApplicationTestBase
{
    private readonly ResetPasswordCommandHandler _handler;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;

    public ResetPasswordCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<RegisterCommandHandler>();

        _handler = new ResetPasswordCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            ConfigurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidReset_ReturnsTrue()
    {
        var user = CreateTestUser(email: "user@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ResetPasswordCommand("user@example.com", "valid-token", "NewPassword123!", "NewPassword123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        UserManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new ResetPasswordCommand("nonexistent@example.com", "token", "NewPassword123!", "NewPassword123!");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidOperationException()
    {
        var user = CreateTestUser(email: "user@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);

        var errors = new[] { new IdentityError { Description = "Invalid token" } };
        UserManagerMock.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var command = new ResetPasswordCommand("user@example.com", "invalid-token", "NewPassword123!", "NewPassword123!");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid token*");
    }

    [Fact]
    public async Task Handle_WeakPassword_ThrowsInvalidOperationException()
    {
        var user = CreateTestUser(email: "user@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);

        var errors = new[] { new IdentityError { Description = "Password too weak" } };
        UserManagerMock.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var command = new ResetPasswordCommand("user@example.com", "valid-token", "weak", "weak");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Password too weak*");
    }
}

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator;

    public ResetPasswordCommandValidatorTests()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new ResetPasswordCommand("user@example.com", "token", "NewPassword123!", "NewPassword123!");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PasswordMismatch_FailsValidation()
    {
        var command = new ResetPasswordCommand("user@example.com", "token", "Password1!", "Password2!");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyPassword_FailsValidation(string password)
    {
        var command = new ResetPasswordCommand("user@example.com", "token", password, password);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShortPassword_FailsValidation()
    {
        var command = new ResetPasswordCommand("user@example.com", "token", "12345", "12345");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region RevokeToken Tests

public class RevokeTokenCommandHandlerTests : ApplicationTestBase
{
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _handler = new RevokeTokenCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsTrue()
    {
        var token = new UserRefreshToken
        {
            Token = "valid-token",
            IsRevoked = false
        };
        var tokens = new List<UserRefreshToken> { token };
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RevokeTokenCommand("valid-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsFalse()
    {
        var tokens = new List<UserRefreshToken>();
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);

        var command = new RevokeTokenCommand("nonexistent-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_ReturnsFalse()
    {
        var token = new UserRefreshToken
        {
            Token = "revoked-token",
            IsRevoked = true
        };
        var tokens = new List<UserRefreshToken> { token };
        var mockTokens = tokens.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockTokens.Object);

        var command = new RevokeTokenCommand("revoked-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }
}

#endregion
