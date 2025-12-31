namespace AuthService.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Unit tests for UserRepository
/// Tests user lookup operations by email and ID
/// </summary>
public class UserRepositoryTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly UserRepository _repository;

    // Test users
    private readonly ApplicationUser _testUser;
    private readonly Guid _testUserId = Guid.NewGuid();
    private const string TestUserEmail = "test@example.com";

    public UserRepositoryTests()
    {
        // Create mock UserManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup test user
        _testUser = new ApplicationUser
        {
            Id = _testUserId,
            Email = TestUserEmail,
            UserName = TestUserEmail,
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        _repository = new UserRepository(_userManagerMock.Object);
    }

    #region FindByEmailAsync Tests

    [Fact]
    public async Task FindByEmailAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(TestUserEmail))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _repository.FindByEmailAsync(TestUserEmail);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testUserId);
        result.Email.Should().Be(TestUserEmail);
    }

    [Fact]
    public async Task FindByEmailAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistingEmail = "nonexistent@example.com";
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(nonExistingEmail))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _repository.FindByEmailAsync(nonExistingEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByEmailAsync_EmptyEmail_ShouldReturnNull()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(string.Empty))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _repository.FindByEmailAsync(string.Empty);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("Test@Example.Com")]
    public async Task FindByEmailAsync_CaseVariations_ShouldCallUserManager(string email)
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _repository.FindByEmailAsync(email);

        // Assert
        _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
    }

    [Theory]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("a@b.c")]
    [InlineData("very.long.email.address@subdomain.example.com")]
    public async Task FindByEmailAsync_VariousEmailFormats_ShouldNotThrow(string email)
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var act = async () => await _repository.FindByEmailAsync(email);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region FindByIdAsync Tests

    [Fact]
    public async Task FindByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByIdAsync(_testUserId.ToString()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _repository.FindByIdAsync(_testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(_testUserId);
        result.Email.Should().Be(TestUserEmail);
    }

    [Fact]
    public async Task FindByIdAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        _userManagerMock
            .Setup(x => x.FindByIdAsync(nonExistingId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _repository.FindByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdAsync_EmptyGuid_ShouldReturnNull()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _userManagerMock
            .Setup(x => x.FindByIdAsync(emptyGuid.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _repository.FindByIdAsync(emptyGuid);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindByIdAsync_ShouldCallUserManagerWithCorrectId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _repository.FindByIdAsync(userId);

        // Assert
        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_MultipleUsers_ShouldReturnCorrectUser()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user1 = new ApplicationUser { Id = user1Id, Email = "user1@example.com" };
        var user2 = new ApplicationUser { Id = user2Id, Email = "user2@example.com" };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(user1Id.ToString()))
            .ReturnsAsync(user1);
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user2Id.ToString()))
            .ReturnsAsync(user2);

        // Act
        var result1 = await _repository.FindByIdAsync(user1Id);
        var result2 = await _repository.FindByIdAsync(user2Id);

        // Assert
        result1.Should().NotBeNull();
        result1!.Email.Should().Be("user1@example.com");
        result2.Should().NotBeNull();
        result2!.Email.Should().Be("user2@example.com");
    }

    #endregion

    #region Interface Compliance Tests

    [Fact]
    public void UserRepository_ShouldImplementIUserRepository()
    {
        // Assert
        _repository.Should().BeAssignableTo<AuthService.Domain.Interfaces.IUserRepository>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task FindByEmailAsync_WithWhitespace_ShouldPassThroughToUserManager()
    {
        // Arrange
        var emailWithSpaces = "  test@example.com  ";
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(emailWithSpaces))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _repository.FindByEmailAsync(emailWithSpaces);

        // Assert - should pass the email as-is to UserManager (UserManager handles normalization)
        _userManagerMock.Verify(x => x.FindByEmailAsync(emailWithSpaces), Times.Once);
    }

    [Fact]
    public async Task Repository_MultipleConcurrentCalls_ShouldHandleCorrectly()
    {
        // Arrange
        var userIds = Enumerable.Range(1, 10).Select(_ => Guid.NewGuid()).ToList();
        foreach (var id in userIds)
        {
            _userManagerMock
                .Setup(x => x.FindByIdAsync(id.ToString()))
                .ReturnsAsync(new ApplicationUser { Id = id, Email = $"{id}@example.com" });
        }

        // Act
        var tasks = userIds.Select(id => _repository.FindByIdAsync(id));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Should().NotBeNull());
        results.Length.Should().Be(10);
    }

    #endregion
}
