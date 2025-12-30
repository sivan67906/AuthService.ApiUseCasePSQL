namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for UserRefreshToken entity
/// </summary>
public class UserRefreshTokenTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void UserRefreshToken_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var token = new UserRefreshToken();

        // Assert
        token.IsDeleted.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
        token.Token.Should().BeEmpty();
        token.ReplacedByToken.Should().BeNull();
    }

    [Fact]
    public void UserRefreshToken_WhenCreated_ShouldHaveNewGuidId()
    {
        // Act
        var token = new UserRefreshToken();

        // Assert
        token.Id.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void UserRefreshToken_WhenUserIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var token = new UserRefreshToken();
        var userId = Guid.NewGuid();

        // Act
        token.UserId = userId;

        // Assert
        token.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserRefreshToken_WhenTokenAssigned_ShouldRetainValue()
    {
        // Arrange
        var token = new UserRefreshToken();
        const string tokenValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        // Act
        token.Token = tokenValue;

        // Assert
        token.Token.Should().Be(tokenValue);
    }

    [Fact]
    public void UserRefreshToken_WhenExpiresAtAssigned_ShouldRetainValue()
    {
        // Arrange
        var token = new UserRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        token.ExpiresAt = expiresAt;

        // Assert
        token.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void UserRefreshToken_WhenIsRevokedSetToTrue_ShouldRetainValue()
    {
        // Arrange
        var token = new UserRefreshToken();

        // Act
        token.IsRevoked = true;

        // Assert
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void UserRefreshToken_WhenReplacedByTokenAssigned_ShouldRetainValue()
    {
        // Arrange
        var token = new UserRefreshToken();
        const string replacementToken = "new-token-value";

        // Act
        token.ReplacedByToken = replacementToken;

        // Assert
        token.ReplacedByToken.Should().Be(replacementToken);
    }

    #endregion

    #region Navigation Property Tests

    [Fact]
    public void UserRefreshToken_WhenUserAssigned_ShouldRetainReference()
    {
        // Arrange
        var token = new UserRefreshToken();
        var user = new ApplicationUser { Email = "test@example.com" };

        // Act
        token.User = user;
        token.UserId = user.Id;

        // Assert
        token.User.Should().Be(user);
    }

    #endregion

    #region Token Lifecycle Tests

    [Fact]
    public void UserRefreshToken_WhenNewTokenCreated_ShouldBeValid()
    {
        // Arrange & Act
        var token = new UserRefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        // Assert
        token.IsRevoked.Should().BeFalse();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void UserRefreshToken_WhenTokenRevoked_ShouldBeInvalid()
    {
        // Arrange
        var token = new UserRefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "old-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        // Act
        token.IsRevoked = true;
        token.ReplacedByToken = "new-refresh-token";

        // Assert
        token.IsRevoked.Should().BeTrue();
        token.ReplacedByToken.Should().NotBeNull();
    }

    [Fact]
    public void UserRefreshToken_WhenTokenExpired_ExpiresAtShouldBeInPast()
    {
        // Arrange
        var token = new UserRefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            IsRevoked = false
        };

        // Assert
        token.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void UserRefreshToken_WhenTokenIsVeryLong_ShouldAcceptValue()
    {
        // Arrange
        var token = new UserRefreshToken();
        var longToken = new string('A', 1000);

        // Act
        token.Token = longToken;

        // Assert
        token.Token.Should().HaveLength(1000);
    }

    [Fact]
    public void UserRefreshToken_WhenExpiresAtIsMinValue_ShouldAcceptValue()
    {
        // Arrange
        var token = new UserRefreshToken();

        // Act
        token.ExpiresAt = DateTime.MinValue;

        // Assert
        token.ExpiresAt.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void UserRefreshToken_WhenExpiresAtIsMaxValue_ShouldAcceptValue()
    {
        // Arrange
        var token = new UserRefreshToken();

        // Act
        token.ExpiresAt = DateTime.MaxValue;

        // Assert
        token.ExpiresAt.Should().Be(DateTime.MaxValue);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void UserRefreshToken_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var token = new UserRefreshToken { Token = "test-token", ExpiresAt = DateTime.UtcNow.AddDays(7) };

        // Act
        token.IsDeleted = true;

        // Assert
        token.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Token Rotation Tests

    [Fact]
    public void UserRefreshToken_WhenRotated_OldTokenShouldReferenceNewToken()
    {
        // Arrange
        var oldToken = new UserRefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "old-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var newToken = new UserRefreshToken
        {
            UserId = oldToken.UserId,
            Token = "new-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act
        oldToken.IsRevoked = true;
        oldToken.ReplacedByToken = newToken.Token;

        // Assert
        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be(newToken.Token);
        newToken.IsRevoked.Should().BeFalse();
    }

    #endregion
}
