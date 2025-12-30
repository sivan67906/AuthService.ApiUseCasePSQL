using AuthService.Application.Features.SetPassword.SetPasswords;

namespace AuthService.Tests.Unit.Application.Features.SetPassword;

public class SetPasswordsCommandHandlerTests : ApplicationTestBase
{
    private readonly SetPasswordsCommandHandler _handler;
    private readonly Mock<ILogger<SetPasswordsCommandHandler>> _loggerMock;

    public SetPasswordsCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<SetPasswordsCommandHandler>();

        _handler = new SetPasswordsCommandHandler(
            UserManagerMock.Object,
            _loggerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidEmails_ReturnsSuccessMessage()
    {
        // Arrange
        var user1 = CreateTestUser(email: "user1@example.com");
        var user2 = CreateTestUser(email: "user2@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user1@example.com"))
            .ReturnsAsync(user1);
        UserManagerMock.Setup(x => x.FindByEmailAsync("user2@example.com"))
            .ReturnsAsync(user2);
        UserManagerMock.Setup(x => x.RemovePasswordAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddPasswordAsync(It.IsAny<ApplicationUser>(), "Welcome@123"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new SetPasswordsCommand(new List<string> { "user1@example.com", "user2@example.com" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Passwords updated successfully");
    }

    [Fact]
    public async Task Handle_SingleEmail_ReturnsSuccessMessage()
    {
        // Arrange
        var user = CreateTestUser(email: "user@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddPasswordAsync(user, "Welcome@123"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new SetPasswordsCommand(new List<string> { "user@example.com" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Passwords updated successfully");
    }

    [Fact]
    public async Task Handle_NonExistentUser_SkipsAndContinues()
    {
        // Arrange
        var existingUser = CreateTestUser(email: "existing@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(existingUser);
        UserManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        UserManagerMock.Setup(x => x.RemovePasswordAsync(existingUser))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddPasswordAsync(existingUser, "Welcome@123"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new SetPasswordsCommand(new List<string> { "existing@example.com", "nonexistent@example.com" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Passwords updated successfully");
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_NullEmails_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new SetPasswordsCommand(null!);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No emails provided*");
    }

    [Fact]
    public async Task Handle_EmptyEmailsList_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new SetPasswordsCommand(new List<string>());

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No emails provided*");
    }

    [Fact]
    public async Task Handle_RemovePasswordFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "user@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to remove password" }));

        var command = new SetPasswordsCommand(new List<string> { "user@example.com" });

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to remove password*");
    }

    [Fact]
    public async Task Handle_AddPasswordFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "user@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddPasswordAsync(user, "Welcome@123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to add password" }));

        var command = new SetPasswordsCommand(new List<string> { "user@example.com" });

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to add password*");
    }

    #endregion
}

public class SetPasswordsCommandValidatorTests
{
    private readonly SetPasswordsCommandValidator _validator;

    public SetPasswordsCommandValidatorTests()
    {
        _validator = new SetPasswordsCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new SetPasswordsCommand(new List<string> { "user@example.com" });
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyEmailsList_FailsValidation()
    {
        var command = new SetPasswordsCommand(new List<string>());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NullEmails_FailsValidation()
    {
        var command = new SetPasswordsCommand(null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
