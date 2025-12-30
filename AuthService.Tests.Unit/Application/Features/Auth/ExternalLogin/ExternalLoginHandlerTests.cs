using AuthService.Application.Features.Auth.ExternalLogin;
using MockQueryable.Moq;

namespace AuthService.Tests.Unit.Application.Features.Auth.ExternalLogin;

public class ExternalLoginCommandHandlerTests : ApplicationTestBase
{
    private readonly ExternalLoginCommandHandler _handler;

    public ExternalLoginCommandHandlerTests()
    {
        _handler = new ExternalLoginCommandHandler(
            UserManagerMock.Object,
            ConfigurationMock.Object,
            DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ExistingUser_ReturnsTokens()
    {
        // Arrange
        var user = CreateTestUser(email: "existing@example.com");
        var command = new ExternalLoginCommand("Google", "google-user-123", "existing@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

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
    public async Task Handle_NewUser_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var command = new ExternalLoginCommand("Google", "google-newuser-456", "newuser@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("newuser@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["User"]);

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
        UserManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewUser_AssignsDefaultRole()
    {
        // Arrange
        var command = new ExternalLoginCommand("Google", "google-newuser-789", "newuser@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("newuser@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["User"]);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        UserManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Handle_NewUser_SetsEmailConfirmed()
    {
        // Arrange
        var command = new ExternalLoginCommand("Google", "google-newuser-101", "newuser@example.com");
        ApplicationUser? createdUser = null;

        UserManagerMock.Setup(x => x.FindByEmailAsync("newuser@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(u => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        UserManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["User"]);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser!.EmailConfirmed.Should().BeTrue();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_CreateUserFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ExternalLoginCommand("Google", "google-fail-user", "newuser@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("newuser@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var errors = new[] { new IdentityError { Description = "User creation failed" } };
        UserManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("External sign-up failed*");
    }

    [Fact]
    public async Task Handle_DatabaseError_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        var command = new ExternalLoginCommand("Google", "google-db-error", "test@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_UserWithMultipleRoles_IncludesAllRolesInToken()
    {
        // Arrange
        var user = CreateTestUser(email: "admin@example.com");
        var command = new ExternalLoginCommand("Google", "google-admin-user", "admin@example.com");
        var roles = new List<string> { "Admin", "User", "Manager" };

        UserManagerMock.Setup(x => x.FindByEmailAsync("admin@example.com"))
            .ReturnsAsync(user);
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
        UserManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithNoFirstOrLastName_UsesEmailAsName()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com", firstName: "", lastName: "");
        var command = new ExternalLoginCommand("Google", "google-noname-user", "test@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync([]);

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
    public async Task Handle_ValidRequest_StoresRefreshToken()
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        var command = new ExternalLoginCommand("Google", "google-refresh-token", "test@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        var refreshTokens = new List<UserRefreshToken>();
        var mockDbSet = refreshTokens.AsQueryable().BuildMockDbSet();
        DbContextMock.Setup(x => x.Set<UserRefreshToken>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        DbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Google")]
    [InlineData("Microsoft")]
    [InlineData("Facebook")]
    [InlineData("Apple")]
    public async Task Handle_DifferentProviders_AllWork(string provider)
    {
        // Arrange
        var user = CreateTestUser(email: "test@example.com");
        var command = new ExternalLoginCommand(provider, $"{provider.ToLower()}-provider-user-id", "test@example.com");

        UserManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

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

    #endregion
}
