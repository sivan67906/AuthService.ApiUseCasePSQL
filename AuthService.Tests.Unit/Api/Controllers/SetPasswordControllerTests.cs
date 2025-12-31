using AuthService.Application.Features.SetPassword.SetPasswords;

namespace AuthService.Tests.Unit.Api.Controllers;

public class SetPasswordControllerTests : ControllerTestBase
{
    private readonly SetPasswordController _controller;

    public SetPasswordControllerTests()
    {
        _controller = new SetPasswordController(MediatorMock.Object);
    }

    #region SetPasswords Tests

    [Fact]
    public async Task SetPasswords_WithValidEmails_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var emails = new List<string>
        {
            "user1@example.com",
            "user2@example.com",
            "user3@example.com"
        };

        var expectedResult = "Passwords updated successfully for 3 users";

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        var response = AssertOkResult<string>(result);
        response!.Data.Should().Contain("3 users");
    }

    [Fact]
    public async Task SetPasswords_WithSingleEmail_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var emails = new List<string> { "user@example.com" };

        var expectedResult = "Passwords updated successfully for 1 user";

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        var response = AssertOkResult<string>(result);
        response!.Data.Should().Contain("1 user");
    }

    [Fact]
    public async Task SetPasswords_WithEmptyList_ReturnsOkWithNoUpdatesMessage()
    {
        // Arrange
        var emails = new List<string>();

        var expectedResult = "No passwords updated - empty email list";

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        var response = AssertOkResult<string>(result);
        response!.Data.Should().Contain("No passwords updated");
    }

    [Fact]
    public async Task SetPasswords_WithInvalidEmails_ReturnsBadRequest()
    {
        // Arrange
        var emails = new List<string>
        {
            "invalid-email",
            "another-invalid"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid email format"));

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task SetPasswords_WithNonExistentUsers_ReturnsBadRequest()
    {
        // Arrange
        var emails = new List<string>
        {
            "nonexistent@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User not found: nonexistent@example.com"));

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task SetPasswords_WithMixedValidAndInvalidEmails_ReturnsBadRequest()
    {
        // Arrange
        var emails = new List<string>
        {
            "valid@example.com",
            "invalid-email",
            "another@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid email format: invalid-email"));

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task SetPasswords_WithDatabaseError_ReturnsBadRequest()
    {
        // Arrange
        var emails = new List<string>
        {
            "user@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task SetPasswords_WithPasswordPolicyViolation_ReturnsBadRequest()
    {
        // Arrange
        var emails = new List<string>
        {
            "user@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Password does not meet policy requirements"));

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task SetPasswords_WithPartialSuccess_ReturnsOkWithPartialMessage()
    {
        // Arrange
        var emails = new List<string>
        {
            "user1@example.com",
            "user2@example.com",
            "user3@example.com"
        };

        var expectedResult = "Passwords updated for 2 of 3 users. Failed: user3@example.com";

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        var response = AssertOkResult<string>(result);
        response!.Data.Should().Contain("2 of 3");
    }

    [Fact]
    public async Task SetPasswords_WithDuplicateEmails_ReturnsOkWithDeduplicatedCount()
    {
        // Arrange
        var emails = new List<string>
        {
            "user@example.com",
            "user@example.com",
            "user@example.com"
        };

        var expectedResult = "Passwords updated successfully for 1 user";

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        var response = AssertOkResult<string>(result);
        response!.Data.Should().Contain("1 user");
    }

    [Fact]
    public async Task SetPasswords_WithLockedAccounts_ReturnsBadRequest()
    {
        // Arrange
        var emails = new List<string>
        {
            "locked@example.com"
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot update password for locked account: locked@example.com"));

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        AssertBadRequestResult<string>(result);
    }

    [Fact]
    public async Task SetPasswords_WithLargeEmailList_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var emails = Enumerable.Range(1, 100)
            .Select(i => $"user{i}@example.com")
            .ToList();

        var expectedResult = "Passwords updated successfully for 100 users";

        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SetPasswords(emails);

        // Assert
        var response = AssertOkResult<string>(result);
        response!.Data.Should().Contain("100 users");
    }

    [Fact]
    public async Task SetPasswords_VerifiesCommandIsSentWithCorrectEmails()
    {
        // Arrange
        var emails = new List<string>
        {
            "user1@example.com",
            "user2@example.com"
        };

        SetPasswordsCommand? capturedCommand = null;
        MediatorMock.Setup(m => m.Send(It.IsAny<SetPasswordsCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<string>, CancellationToken>((command, _) => capturedCommand = command as SetPasswordsCommand)
            .ReturnsAsync("Success");

        // Act
        await _controller.SetPasswords(emails);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.Emails.Should().BeEquivalentTo(emails);
    }

    #endregion
}
