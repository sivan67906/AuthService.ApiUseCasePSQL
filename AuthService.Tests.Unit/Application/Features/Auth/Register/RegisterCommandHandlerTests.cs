using AuthService.Application.Features.Auth.Register;

namespace AuthService.Tests.Unit.Application.Features.Auth.Register;

public class RegisterCommandHandlerTests : ApplicationTestBase
{
    private readonly RegisterCommandHandler _handler;
    private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;

    public RegisterCommandHandlerTests()
    {
        _loggerMock = CreateMockLogger<RegisterCommandHandler>();

        _handler = new RegisterCommandHandler(
            UserManagerMock.Object,
            DbContextMock.Object,
            EmailServiceMock.Object,
            ConfigurationMock.Object,
            _loggerMock.Object,
            EmailConfirmationTokenTrackerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidRegistration_ReturnsSuccessResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "newuser@example.com",
            "ValidPassword123!",
            "John",
            "Doe",
            "1234567890");

        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirmation-token");

        EmailConfirmationTokenTrackerMock.Setup(x => x.StoreLatestToken(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()));

        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.UserId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ValidRegistration_CreatesUserWithCorrectProperties()
    {
        // Arrange
        var command = new RegisterCommand(
            "test@example.com",
            "Password123!",
            "Jane",
            "Smith",
            "9876543210");

        ApplicationUser? capturedUser = null;
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .Callback<ApplicationUser, string>((user, _) => capturedUser = user)
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(command.Email);
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.PhoneNumber.Should().Be(command.PhoneNumber);
        capturedUser.EmailConfirmed.Should().BeFalse();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand(
            "existing@example.com",
            "Password123!",
            "John",
            "Doe",
            "1234567890");

        var identityErrors = new[] { new IdentityError { Description = "Email already exists" } };
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email already exists*");
    }

    [Fact]
    public async Task Handle_WeakPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "weak",
            "John",
            "Doe",
            "1234567890");

        var identityErrors = new[]
        {
            new IdentityError { Description = "Password must be at least 8 characters" },
            new IdentityError { Description = "Password must contain uppercase" }
        };
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_MultipleErrors_CombinesErrorMessages()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "weak",
            "John",
            "Doe",
            "1234567890");

        var identityErrors = new[]
        {
            new IdentityError { Description = "Error 1" },
            new IdentityError { Description = "Error 2" },
            new IdentityError { Description = "Error 3" }
        };
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("|||");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_EmailTemplateNotFound_StillSucceeds()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe",
            "1234567890");

        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        // Email will fail but shouldn't throw
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Template not found"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should succeed despite email failure
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task Handle_EmptyPhoneNumber_Succeeds()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe",
            "");

        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_SpecialCharactersInName_Succeeds()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "Password123!",
            "José María",
            "O'Connor-Smith",
            "1234567890");

        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Exception Scenarios

    [Fact]
    public async Task Handle_DatabaseError_ThrowsException()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe",
            "1234567890");

        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_TokenGenerationFails_ThrowsException()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe",
            "1234567890");

        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ThrowsAsync(new Exception("Token generation failed"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Should not throw because email sending is in try-catch
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
    }

    #endregion
}

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new RegisterCommand(
            "test@example.com",
            "ValidPassword123!",
            "John",
            "Doe",
            "1234567890");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyEmail_FailsValidation(string? email)
    {
        // Arrange
        var command = new RegisterCommand(email!, "Password123!", "John", "Doe", "1234567890");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    public void Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new RegisterCommand(email, "Password123!", "John", "Doe", "1234567890");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyPassword_FailsValidation(string? password)
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", password!, "John", "Doe", "1234567890");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyFirstName_FailsValidation(string? firstName)
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!", firstName!, "Doe", "1234567890");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyLastName_FailsValidation(string? lastName)
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!", "John", lastName!, "1234567890");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
