using AuthService.Application.Features.Profile.GetProfile;

namespace AuthService.Tests.Unit.Application.Features.Profile.GetProfile;

/// <summary>
/// Unit tests for GetProfileQueryHandler
/// </summary>
public class GetProfileQueryHandlerTests : ApplicationTestBase
{
    private readonly GetProfileQueryHandler _handler;

    public GetProfileQueryHandlerTests()
    {
        // GetProfileQueryHandler takes only UserManager<ApplicationUser>
        _handler = new GetProfileQueryHandler(UserManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ReturnsUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(
            id: userId,
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            emailConfirmed: true,
            twoFactorEnabled: false);

        UserManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var query = new GetProfileQuery(userId.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.EmailConfirmed.Should().BeTrue();
        result.TwoFactorEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        UserManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var query = new GetProfileQuery(userId.ToString());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidGuid_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetProfileQuery("invalid-guid");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TwoFactorEnabled_SetsTwoFactorTypeToEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, twoFactorEnabled: true);
        user.AuthenticatorEnabled = false;

        UserManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var query = new GetProfileQuery(userId.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TwoFactorEnabled.Should().BeTrue();
        result.TwoFactorType.Should().Be("Email");
    }

    [Fact]
    public async Task Handle_AuthenticatorEnabled_SetsTwoFactorTypeToAuthenticator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, twoFactorEnabled: true);
        user.AuthenticatorEnabled = true;

        UserManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var query = new GetProfileQuery(userId.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TwoFactorEnabled.Should().BeTrue();
        result.TwoFactorType.Should().Be("Authenticator");
    }

    [Fact]
    public async Task Handle_NoTwoFactor_SetsTwoFactorTypeToNone()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(id: userId, twoFactorEnabled: false);

        UserManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var query = new GetProfileQuery(userId.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TwoFactorEnabled.Should().BeFalse();
        result.TwoFactorType.Should().Be("None");
    }
}
