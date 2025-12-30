using AuthService.Application.Features.Auth.EmailConfirmation;

namespace AuthService.Tests.Unit.Application.Features.Auth.EmailConfirmation;

#region ConfirmEmail Tests

public class ConfirmEmailCommandHandlerTests : ApplicationTestBase
{
    private readonly ConfirmEmailCommandHandler _handler;

    public ConfirmEmailCommandHandlerTests()
    {
        _handler = new ConfirmEmailCommandHandler(
            UserManagerMock.Object,
            EmailConfirmationTokenTrackerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ConfirmEmailCommand("nonexistent@example.com", "token");

        UserManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_EmailAlreadyConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: true);
        var command = new ConfirmEmailCommand("test@example.com", "token");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email is already confirmed*");
    }

    [Fact]
    public async Task Handle_NewFormatToken_UserIdMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var wrongUserId = Guid.NewGuid();
        var tokenTimestamp = DateTime.UtcNow.ToString("o");
        var expiry = DateTime.UtcNow.AddHours(1).ToString("o");
        var token = $"{wrongUserId}|{tokenTimestamp}|{expiry}|standardToken";

        var command = new ConfirmEmailCommand("test@example.com", token);

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid confirmation token.");
    }

    [Fact]
    public async Task Handle_NewFormatToken_Expired_ThrowsInvalidOperationException()
    {
        // Arrange - Create fresh mocks for isolation
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var tokenTrackerMock = new Mock<IEmailConfirmationTokenTracker>();

        var handler = new ConfirmEmailCommandHandler(userManagerMock.Object, tokenTrackerMock.Object);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "expiredtoken@example.com",
            UserName = "expiredtoken@example.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Use a date far in the past to ensure expiry check works regardless of timezone
        var pastDate = DateTime.UtcNow.AddDays(-10);
        var expiredDate = DateTime.UtcNow.AddDays(-5);
        var tokenTimestamp = pastDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var expiry = expiredDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var token = $"{user.Id}|{tokenTimestamp}|{expiry}|standardToken";

        var command = new ConfirmEmailCommand("expiredtoken@example.com", token);

        userManagerMock.Setup(x => x.FindByEmailAsync("expiredtoken@example.com"))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_NewFormatToken_SupersededToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var tokenTimestamp = DateTime.UtcNow.ToString("o");
        var expiry = DateTime.UtcNow.AddHours(1).ToString("o");
        var token = $"{user.Id}|{tokenTimestamp}|{expiry}|standardToken";

        var command = new ConfirmEmailCommand("test@example.com", token);

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        EmailConfirmationTokenTrackerMock.Setup(x => x.ValidateToken("test@example.com", "standardToken"))
            .Returns(false);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*superseded*");
    }

    [Fact]
    public async Task Handle_NewFormatToken_Valid_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var tokenTimestamp = DateTime.UtcNow.ToString("o");
        var expiry = DateTime.UtcNow.AddHours(1).ToString("o");
        var standardToken = "standardToken";
        var token = $"{user.Id}|{tokenTimestamp}|{expiry}|{standardToken}";

        var command = new ConfirmEmailCommand("test@example.com", token);

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        EmailConfirmationTokenTrackerMock.Setup(x => x.ValidateToken("test@example.com", standardToken))
            .Returns(true);
        UserManagerMock.Setup(x => x.ConfirmEmailAsync(user, standardToken))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        EmailConfirmationTokenTrackerMock.Verify(x => x.ClearToken("test@example.com"), Times.Once);
    }

    [Fact]
    public async Task Handle_OldFormatToken_Valid_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var expiry = DateTime.UtcNow.AddHours(1).ToString("o");
        var standardToken = "standardToken";
        var token = $"{user.Id}|{expiry}|{standardToken}";

        var command = new ConfirmEmailCommand("test@example.com", token);

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.ConfirmEmailAsync(user, standardToken))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FallbackToken_Valid_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var token = "simpleToken";

        var command = new ConfirmEmailCommand("test@example.com", token);

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConfirmEmailFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var token = "invalidToken";

        var command = new ConfirmEmailCommand("test@example.com", token);

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        var errors = new[] { new IdentityError { Description = "Invalid token" } };
        UserManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Confirm email failed*");
    }
}

#endregion

#region SendEmailConfirmation Tests

public class SendEmailConfirmationCommandHandlerTests : ApplicationTestBase
{
    private readonly SendEmailConfirmationCommandHandler _handler;

    public SendEmailConfirmationCommandHandlerTests()
    {
        _handler = new SendEmailConfirmationCommandHandler(
            UserManagerMock.Object,
            EmailServiceMock.Object,
            EmailResendThrottlingServiceMock.Object,
            EmailConfirmationTokenTrackerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsTrue()
    {
        // Arrange
        var command = new SendEmailConfirmationCommand("nonexistent@example.com", "https://callback.url");

        UserManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Returns true to prevent email enumeration
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmailAlreadyConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: true);
        var command = new SendEmailConfirmationCommand("test@example.com", "https://callback.url");

        UserManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already confirmed*");
    }

    [Fact]
    public async Task Handle_Throttled_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var command = new SendEmailConfirmationCommand("test@example.com", "https://callback.url");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        EmailResendThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((false, "Too many attempts. Please wait.", TimeSpan.FromSeconds(60)));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Too many attempts*");
    }

    [Fact]
    public async Task Handle_ValidRequest_SendsEmail()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var command = new SendEmailConfirmationCommand("test@example.com", "https://callback.url");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        EmailResendThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
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
    public async Task Handle_ValidRequest_RecordsResendAttempt()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var command = new SendEmailConfirmationCommand("test@example.com", "https://callback.url");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        EmailResendThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        EmailResendThrottlingServiceMock.Verify(x => x.RecordResendAttempt("test@example.com"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_StoresToken()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", emailConfirmed: false);
        var command = new SendEmailConfirmationCommand("test@example.com", "https://callback.url");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        EmailResendThrottlingServiceMock.Setup(x => x.CanResend("test@example.com"))
            .Returns((true, null, null));
        UserManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        EmailServiceMock.Setup(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        EmailConfirmationTokenTrackerMock.Verify(x => x.StoreLatestToken("test@example.com", "confirmation-token", It.IsAny<DateTime>()), Times.Once);
    }
}

#endregion
