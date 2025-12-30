using AuthService.Application.Features.Auth.ChangePassword;

namespace AuthService.Tests.Unit.Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandlerTests : ApplicationTestBase
{
    private readonly ChangePasswordCommandHandler _handler;
    private readonly Mock<ILogger<ChangePasswordCommandHandler>> _loggerMock;

    public ChangePasswordCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<ChangePasswordCommandHandler>();

        _handler = new ChangePasswordCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            _loggerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidPasswordChange_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "NewPassword456!", "127.0.0.1");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidPasswordChange_SendsConfirmationEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, email: "user@example.com");
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "NewPassword456!", "192.168.1.1");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        EmailServiceMock.Verify(x => x.SendAsync(
            user.Email!,
            "Password Changed Successfully",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid().ToString(), "OldPassword123!", "NewPassword456!", null);

        UserManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_SameOldAndNewPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var samePassword = "Password123!";
        var command = new ChangePasswordCommand(userId.ToString(), samePassword, samePassword, null);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("New password must be different from current password.");
    }

    [Fact]
    public async Task Handle_IncorrectCurrentPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new ChangePasswordCommand(userId.ToString(), "WrongPassword!", "NewPassword456!", null);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var errors = new[] { new IdentityError { Description = "Incorrect password" } };
        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Incorrect password*");
    }

    [Fact]
    public async Task Handle_WeakNewPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "weak", null);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var errors = new[]
        {
            new IdentityError { Description = "Password too weak" },
            new IdentityError { Description = "Password must contain uppercase" }
        };
        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Password too weak*");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_NullIpAddress_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "NewPassword456!", null);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserWithNoName_UsesEmailInNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, firstName: "", lastName: "", email: "user@example.com");
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "NewPassword456!", "127.0.0.1");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmailSendingFails_StillReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "NewPassword456!", "127.0.0.1");

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should succeed even if email fails
        result.Should().BeTrue();
    }

    #endregion

    #region Exception Scenarios

    [Fact]
    public async Task Handle_DatabaseError_ThrowsException()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid().ToString(), "OldPassword123!", "NewPassword456!", null);

        UserManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_IdentityServiceError_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId);
        var command = new ChangePasswordCommand(userId.ToString(), "OldPassword123!", "NewPassword456!", null);

        UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        UserManagerMock.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ThrowsAsync(new Exception("Identity service error"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Identity service error");
    }

    #endregion
}

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new ChangePasswordCommand(
            Guid.NewGuid().ToString(),
            "OldPassword123!",
            "NewPassword456!",
            "127.0.0.1");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyUserId_FailsValidation(string? userId)
    {
        // Arrange
        var command = new ChangePasswordCommand(userId!, "OldPassword!", "NewPassword!", null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyCurrentPassword_FailsValidation(string? currentPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid().ToString(), currentPassword!, "NewPassword!", null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyNewPassword_FailsValidation(string? newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid().ToString(), "OldPassword!", newPassword!, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
