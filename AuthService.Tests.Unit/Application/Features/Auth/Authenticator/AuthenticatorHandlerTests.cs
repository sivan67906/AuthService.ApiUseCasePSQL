using AuthService.Application.Features.Auth.Authenticator;
using OtpNet;

namespace AuthService.Tests.Unit.Application.Features.Auth.Authenticator;

#region SetupAuthenticator Tests

public class SetupAuthenticatorCommandHandlerTests : ApplicationTestBase
{
    private readonly SetupAuthenticatorCommandHandler _handler;

    public SetupAuthenticatorCommandHandlerTests()
    {
        _handler = new SetupAuthenticatorCommandHandler(UserManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsSetupDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new SetupAuthenticatorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SecretKey.Should().NotBeNullOrEmpty();
        result.QrCodeUri.Should().NotBeNullOrEmpty();
        result.ManualEntryKey.Should().NotBeNullOrEmpty();
        result.QrCodeUri.Should().Contain("otpauth://totp/");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new SetupAuthenticatorCommand("nonexistent-id");

        UserManagerMock.Setup(x => x.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_ValidUser_StoresSecretKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new SetupAuthenticatorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.AuthenticatorSecretKey.Should().NotBeNullOrEmpty();
        UserManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUser_QrCodeContainsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "john@example.com");
        var command = new SetupAuthenticatorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.QrCodeUri.Should().Contain("john%40example.com");
    }

    [Fact]
    public async Task Handle_ValidUser_ManualEntryKeyHasSpaces()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new SetupAuthenticatorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ManualEntryKey.Should().Contain(" ");
    }
}

#endregion

#region VerifyAuthenticatorCode Tests

public class VerifyAuthenticatorCodeCommandHandlerTests : ApplicationTestBase
{
    private readonly VerifyAuthenticatorCodeCommandHandler _handler;

    public VerifyAuthenticatorCodeCommandHandlerTests()
    {
        _handler = new VerifyAuthenticatorCodeCommandHandler(UserManagerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new VerifyAuthenticatorCodeCommand("nonexistent-id", "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_NoSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        user.AuthenticatorSecretKey = null;
        user.AuthenticatorEnabled = false;
        var command = new VerifyAuthenticatorCodeCommand(userId.ToString(), "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Authenticator*not enabled*");
    }

    [Fact]
    public async Task Handle_ValidCode_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);

        // Generate a valid secret and code
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);
        user.AuthenticatorSecretKey = base32Secret;
        user.AuthenticatorEnabled = true;

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var command = new VerifyAuthenticatorCodeCommand(userId.ToString(), validCode);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);

        var secretKey = KeyGeneration.GenerateRandomKey(20);
        user.AuthenticatorSecretKey = Base32Encoding.ToString(secretKey);
        user.AuthenticatorEnabled = true;

        var command = new VerifyAuthenticatorCodeCommand(userId.ToString(), "000000");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid authentication code*");
    }
}

#endregion

#region EnableAuthenticator Tests

public class EnableAuthenticatorCommandHandlerTests : ApplicationTestBase
{
    private readonly EnableAuthenticatorCommandHandler _handler;
    private readonly Mock<ILogger<EnableAuthenticatorCommandHandler>> _loggerMock;

    public EnableAuthenticatorCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<EnableAuthenticatorCommandHandler>();
        _handler = new EnableAuthenticatorCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new EnableAuthenticatorCommand("nonexistent-id", "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_NoSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        user.AuthenticatorSecretKey = null;
        var command = new EnableAuthenticatorCommand(userId.ToString(), "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not been set up*");
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        user.AuthenticatorSecretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var command = new EnableAuthenticatorCommand(userId.ToString(), "000000");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid verification code*");
    }

    [Fact]
    public async Task Handle_ValidCode_EnablesAuthenticator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");

        var secretKey = KeyGeneration.GenerateRandomKey(20);
        user.AuthenticatorSecretKey = Base32Encoding.ToString(secretKey);

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var command = new EnableAuthenticatorCommand(userId.ToString(), validCode);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.AuthenticatorEnabled.Should().BeTrue();
        user.TwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCode_SendsConfirmationEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");

        var secretKey = KeyGeneration.GenerateRandomKey(20);
        user.AuthenticatorSecretKey = Base32Encoding.ToString(secretKey);

        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp();

        var command = new EnableAuthenticatorCommand(userId.ToString(), validCode);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        EmailServiceMock.Verify(x => x.SendAsync(
            user.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

#endregion

#region DisableAuthenticator Tests

public class DisableAuthenticatorCommandHandlerTests : ApplicationTestBase
{
    private readonly DisableAuthenticatorCommandHandler _handler;
    private readonly Mock<ILogger<DisableAuthenticatorCommandHandler>> _loggerMock;

    public DisableAuthenticatorCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<DisableAuthenticatorCommandHandler>();
        _handler = new DisableAuthenticatorCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            ConfigurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DisableAuthenticatorCommand("nonexistent-id");

        UserManagerMock.Setup(x => x.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_ValidRequest_DisablesAuthenticator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        user.AuthenticatorEnabled = true;
        user.AuthenticatorSecretKey = "SECRETKEY";
        user.TwoFactorEnabled = true;

        var command = new DisableAuthenticatorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.AuthenticatorEnabled.Should().BeFalse();
        user.AuthenticatorSecretKey.Should().BeNull();
        user.TwoFactorEnabled.Should().BeFalse();
    }
}

#endregion

#region GetAuthenticatorStatus Tests

public class GetAuthenticatorStatusQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAuthenticatorStatusQueryHandler _handler;

    public GetAuthenticatorStatusQueryHandlerTests()
    {
        _handler = new GetAuthenticatorStatusQueryHandler(UserManagerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new GetAuthenticatorStatusQuery("nonexistent-id");

        UserManagerMock.Setup(x => x.FindByIdAsync("nonexistent-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_AuthenticatorEnabled_ReturnsTrueStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        user.AuthenticatorEnabled = true;

        var query = new GetAuthenticatorStatusQuery(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AuthenticatorDisabled_ReturnsFalseStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        user.AuthenticatorEnabled = false;

        var query = new GetAuthenticatorStatusQuery(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsEnabled.Should().BeFalse();
    }
}

#endregion
