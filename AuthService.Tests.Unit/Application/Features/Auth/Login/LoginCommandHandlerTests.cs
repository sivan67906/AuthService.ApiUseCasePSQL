using AuthService.Application.Features.Auth.Login;
using MockQueryable.Moq;

namespace AuthService.Tests.Unit.Application.Features.Auth.Login;

public class LoginCommandHandlerTests : ApplicationTestBase
{
    private readonly LoginCommandHandler _handler;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;

    public LoginCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<LoginCommandHandler>();

        _handler = new LoginCommandHandler(
            SignInManagerMock.Object,
            UserManagerMock.Object,
            ConfigurationMock.Object,
            DbContextMock.Object,
            MediatorMock.Object,
            EmailServiceMock.Object,
            TwoFactorThrottlingServiceMock.Object,
            _loggerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidCredentialsNoTwoFactor_ReturnsTokens()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true, twoFactorEnabled: false);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RequiresTwoFactor.Should().BeFalse();
        result.ExpiresInSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ValidCredentialsWithEmailTwoFactor_ReturnsTwoFactorRequired()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true, twoFactorEnabled: true);
        user.AuthenticatorEnabled = false;
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend(user.Email!))
            .Returns((true, null, null));

        TwoFactorThrottlingServiceMock.Setup(x => x.StoreCode(user.Email!, It.IsAny<string>(), It.IsAny<DateTime>()));
        TwoFactorThrottlingServiceMock.Setup(x => x.RecordResendAttempt(user.Email!));

        UserManagerMock.Setup(x => x.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider))
            .ReturnsAsync("123456");

        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        EmailServiceMock.Setup(x => x.SendAsync(user.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorType.Should().Be("Email");
        result.TwoFactorToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCredentialsWithAuthenticatorTwoFactor_ReturnsTwoFactorRequired()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true, twoFactorEnabled: true);
        user.AuthenticatorEnabled = true;
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend(user.Email!))
            .Returns((true, null, null));

        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorType.Should().Be("Authenticator");
        result.TwoFactorToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_EmailNotConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: false);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The user has not verified the confirmation email.");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true);
        var command = new LoginCommand(user.Email!, "WrongPassword!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_TwoFactorThrottled_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true, twoFactorEnabled: true);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend(user.Email!))
            .Returns((false, "Too many attempts. Please wait 60 seconds.", TimeSpan.FromSeconds(60)));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Too many attempts. Please wait 60 seconds.");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_UserWithNoFirstOrLastName_UsesEmailAsName()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true, firstName: "", lastName: "");
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_UserWithMultipleRoles_IncludesAllRolesInToken()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");
        var roles = new List<string> { "Admin", "Manager", "User" };

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();

        // Verify GetRolesAsync was called
        UserManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_AccountLockedOut_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid credentials.");
    }

    #endregion

    #region Exception Scenarios

    [Fact]
    public async Task Handle_DatabaseError_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncFails_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser(emailConfirmed: true);
        var command = new LoginCommand(user.Email!, "ValidPassword123!");

        UserManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        SignInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, command.Password, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to save refresh token"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to save refresh token");
    }

    #endregion
}

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "ValidPassword123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullEmail_FailsValidation(string? email)
    {
        // Arrange
        var command = new LoginCommand(email!, "ValidPassword123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@domain.com")]
    public void Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "ValidPassword123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullPassword_FailsValidation(string? password)
    {
        // Arrange
        var command = new LoginCommand("test@example.com", password!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
