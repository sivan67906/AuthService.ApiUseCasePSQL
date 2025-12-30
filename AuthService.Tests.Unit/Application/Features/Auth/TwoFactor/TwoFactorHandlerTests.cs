using AuthService.Application.Features.Auth.TwoFactor;

namespace AuthService.Tests.Unit.Application.Features.Auth.TwoFactor;

#region EnableTwoFactor Tests

public class EnableTwoFactorCommandHandlerTests : ApplicationTestBase
{
    private readonly EnableTwoFactorCommandHandler _handler;
    private readonly Mock<ILogger<EnableTwoFactorCommandHandler>> _loggerMock;

    public EnableTwoFactorCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<EnableTwoFactorCommandHandler>();
        _handler = new EnableTwoFactorCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new EnableTwoFactorCommand("invalid-guid");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid user identifier.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new EnableTwoFactorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_TwoFactorAlreadyEnabled_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com", twoFactorEnabled: true);
        var command = new EnableTwoFactorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        UserManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidRequest_EnablesTwoFactor()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com", twoFactorEnabled: false);
        var command = new EnableTwoFactorCommand(userId.ToString());

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
        user.TwoFactorEnabled.Should().BeTrue();
        UserManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com", twoFactorEnabled: false);
        var command = new EnableTwoFactorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to enable*");
    }
}

public class EnableTwoFactorCommandValidatorTests
{
    private readonly EnableTwoFactorCommandValidator _validator;

    public EnableTwoFactorCommandValidatorTests()
    {
        _validator = new EnableTwoFactorCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new EnableTwoFactorCommand(Guid.NewGuid().ToString());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyUserId_FailsValidation(string userId)
    {
        var command = new EnableTwoFactorCommand(userId);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region DisableTwoFactor Tests

public class DisableTwoFactorCommandHandlerTests : ApplicationTestBase
{
    private readonly DisableTwoFactorCommandHandler _handler;
    private readonly Mock<ILogger<DisableTwoFactorCommandHandler>> _loggerMock;

    public DisableTwoFactorCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<DisableTwoFactorCommandHandler>();
        _handler = new DisableTwoFactorCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            ConfigurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DisableTwoFactorCommand("invalid-guid");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid user identifier.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DisableTwoFactorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_TwoFactorNotEnabled_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com", twoFactorEnabled: false);
        var command = new DisableTwoFactorCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        UserManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidRequest_DisablesTwoFactor()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com", twoFactorEnabled: true);
        var command = new DisableTwoFactorCommand(userId.ToString());

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
        user.TwoFactorEnabled.Should().BeFalse();
        UserManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}

public class DisableTwoFactorCommandValidatorTests
{
    private readonly DisableTwoFactorCommandValidator _validator;

    public DisableTwoFactorCommandValidatorTests()
    {
        _validator = new DisableTwoFactorCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new DisableTwoFactorCommand(Guid.NewGuid().ToString());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyUserId_FailsValidation(string userId)
    {
        var command = new DisableTwoFactorCommand(userId);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region GenerateTwoFactorCode Tests

public class GenerateTwoFactorCodeCommandHandlerTests : ApplicationTestBase
{
    private readonly GenerateTwoFactorCodeCommandHandler _handler;
    private readonly Mock<ILogger<GenerateTwoFactorCodeCommandHandler>> _loggerMock;

    public GenerateTwoFactorCodeCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<GenerateTwoFactorCodeCommandHandler>();
        _handler = new GenerateTwoFactorCodeCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            TwoFactorThrottlingServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new GenerateTwoFactorCodeCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_Throttled_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new GenerateTwoFactorCodeCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend(user.Email!))
            .Returns((false, "Too many attempts", TimeSpan.FromSeconds(60)));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_SendsCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new GenerateTwoFactorCodeCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend(user.Email!))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider))
            .ReturnsAsync("123456");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        EmailServiceMock.Verify(x => x.SendAsync(
            user.Email!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_StoresAndRecordsAttempt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new GenerateTwoFactorCodeCommand(userId.ToString());

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.CanResend(user.Email!))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider))
            .ReturnsAsync("123456");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        TwoFactorThrottlingServiceMock.Verify(x => x.StoreCode(user.Email!, "123456", It.IsAny<DateTime>()), Times.Once);
        TwoFactorThrottlingServiceMock.Verify(x => x.RecordResendAttempt(user.Email!), Times.Once);
    }
}

#endregion

#region VerifyTwoFactorCode Tests

public class VerifyTwoFactorCodeCommandHandlerTests : ApplicationTestBase
{
    private readonly VerifyTwoFactorCodeCommandHandler _handler;

    public VerifyTwoFactorCodeCommandHandlerTests()
    {
        _handler = new VerifyTwoFactorCodeCommandHandler(
            UserManagerMock.Object,
            TwoFactorThrottlingServiceMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyTwoFactorCodeCommand(userId.ToString(), "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new VerifyTwoFactorCodeCommand(userId.ToString(), "000000");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.ValidateCode(user.Email!, "000000"))
            .Returns(false);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid 2FA code*");
    }

    [Fact]
    public async Task Handle_ValidCode_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new VerifyTwoFactorCodeCommand(userId.ToString(), "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.ValidateCode(user.Email!, "123456"))
            .Returns(true);
        UserManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, "123456"))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCode_ClearsAttempts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "test@example.com");
        var command = new VerifyTwoFactorCodeCommand(userId.ToString(), "123456");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        TwoFactorThrottlingServiceMock.Setup(x => x.ValidateCode(user.Email!, "123456"))
            .Returns(true);
        UserManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, "123456"))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        TwoFactorThrottlingServiceMock.Verify(x => x.ClearAttempts(user.Email!), Times.Once);
    }
}

#endregion
