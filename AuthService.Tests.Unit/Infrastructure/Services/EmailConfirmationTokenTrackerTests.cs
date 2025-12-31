namespace AuthService.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for EmailConfirmationTokenTracker service
/// Tests token storage, validation, expiration, and cleanup functionality
/// </summary>
public class EmailConfirmationTokenTrackerTests
{
    private readonly EmailConfirmationTokenTracker _tracker;

    public EmailConfirmationTokenTrackerTests()
    {
        _tracker = new EmailConfirmationTokenTracker();
    }

    #region StoreLatestToken Tests

    [Fact]
    public void StoreLatestToken_WithValidEmailAndToken_ShouldStoreSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-token-12345";
        var timestamp = DateTime.UtcNow;

        // Act
        _tracker.StoreLatestToken(email, token, timestamp);

        // Assert - token should be valid after storing
        var isValid = _tracker.ValidateToken(email, token);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void StoreLatestToken_WithUpperCaseEmail_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var email = "TEST@EXAMPLE.COM";
        var token = "valid-token-12345";
        var timestamp = DateTime.UtcNow;

        // Act
        _tracker.StoreLatestToken(email, token, timestamp);

        // Assert - should validate with lowercase email
        var isValid = _tracker.ValidateToken("test@example.com", token);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void StoreLatestToken_WithNewToken_ShouldReplaceOldToken()
    {
        // Arrange
        var email = "test@example.com";
        var oldToken = "old-token-12345";
        var newToken = "new-token-67890";
        var timestamp = DateTime.UtcNow;

        // Act
        _tracker.StoreLatestToken(email, oldToken, timestamp);
        _tracker.StoreLatestToken(email, newToken, timestamp);

        // Assert
        var oldTokenValid = _tracker.ValidateToken(email, oldToken);
        var newTokenValid = _tracker.ValidateToken(email, newToken);

        oldTokenValid.Should().BeFalse();
        newTokenValid.Should().BeTrue();
    }

    [Fact]
    public void StoreLatestToken_MultipleEmails_ShouldStoreIndependently()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var token1 = "token-user1";
        var token2 = "token-user2";
        var timestamp = DateTime.UtcNow;

        // Act
        _tracker.StoreLatestToken(email1, token1, timestamp);
        _tracker.StoreLatestToken(email2, token2, timestamp);

        // Assert
        _tracker.ValidateToken(email1, token1).Should().BeTrue();
        _tracker.ValidateToken(email2, token2).Should().BeTrue();
        _tracker.ValidateToken(email1, token2).Should().BeFalse();
        _tracker.ValidateToken(email2, token1).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    public void StoreLatestToken_WithVariousEmails_ShouldHandleCorrectly(string email)
    {
        // Arrange
        var token = "test-token";
        var timestamp = DateTime.UtcNow;

        // Act & Assert - should not throw
        var act = () => _tracker.StoreLatestToken(email, token, timestamp);
        act.Should().NotThrow();
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-token";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var validToken = "valid-token";
        var invalidToken = "invalid-token";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, validToken, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithNoStoredToken_ShouldReturnTrue()
    {
        // Arrange - no token stored
        var email = "new@example.com";
        var token = "any-token";

        // Act
        var result = _tracker.ValidateToken(email, token);

        // Assert - backward compatibility: allow if no token tracked
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = "expired-token";
        var timestamp = DateTime.UtcNow.AddHours(-2); // 2 hours ago (expired)
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithTokenAtExpirationBoundary_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = "boundary-token";
        var timestamp = DateTime.UtcNow.AddHours(-1).AddSeconds(-1); // Just past 1 hour
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithTokenJustBeforeExpiration_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-token";
        var timestamp = DateTime.UtcNow.AddMinutes(-59); // Just under 1 hour
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_CaseInsensitiveEmail_ShouldValidate()
    {
        // Arrange
        var email = "Test@Example.COM";
        var token = "test-token";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        var result = _tracker.ValidateToken("test@example.com", token);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ClearToken Tests

    [Fact]
    public void ClearToken_WithExistingToken_ShouldRemove()
    {
        // Arrange
        var email = "test@example.com";
        var token = "test-token";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        _tracker.ClearToken(email);

        // Assert - after clearing, any token should be valid (backward compatibility)
        var result = _tracker.ValidateToken(email, "any-token");
        result.Should().BeTrue();
    }

    [Fact]
    public void ClearToken_WithNonExistingToken_ShouldNotThrow()
    {
        // Arrange
        var email = "nonexisting@example.com";

        // Act & Assert
        var act = () => _tracker.ClearToken(email);
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearToken_CaseInsensitiveEmail_ShouldClear()
    {
        // Arrange
        var email = "Test@Example.COM";
        var token = "test-token";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        _tracker.ClearToken("test@example.com");

        // Assert
        var result = _tracker.ValidateToken(email, "any-token");
        result.Should().BeTrue();
    }

    [Fact]
    public void ClearToken_ShouldOnlyClearSpecificEmail()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var token1 = "token1";
        var token2 = "token2";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email1, token1, timestamp);
        _tracker.StoreLatestToken(email2, token2, timestamp);

        // Act
        _tracker.ClearToken(email1);

        // Assert
        _tracker.ValidateToken(email2, token2).Should().BeTrue();
    }

    #endregion

    #region CleanupOldEntries Tests

    [Fact]
    public void CleanupOldEntries_WithOldEntries_ShouldRemove()
    {
        // Arrange
        var email = "old@example.com";
        var token = "old-token";
        var timestamp = DateTime.UtcNow.AddHours(-25); // Older than 24 hours
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        _tracker.CleanupOldEntries();

        // Assert - after cleanup, any token should be valid (no tracking)
        var result = _tracker.ValidateToken(email, "any-token");
        result.Should().BeTrue();
    }

    [Fact]
    public void CleanupOldEntries_WithRecentEntries_ShouldKeep()
    {
        // Arrange
        var email = "recent@example.com";
        var token = "recent-token";
        var timestamp = DateTime.UtcNow.AddHours(-23); // Less than 24 hours
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        _tracker.CleanupOldEntries();

        // Assert - recent entries should be kept
        // Note: Token is still expired for validation (1 hour), but entry exists
        var invalidToken = _tracker.ValidateToken(email, "wrong-token");
        invalidToken.Should().BeFalse(); // Entry still exists, so wrong token fails
    }

    [Fact]
    public void CleanupOldEntries_MixedAges_ShouldOnlyRemoveOld()
    {
        // Arrange
        var oldEmail = "old@example.com";
        var recentEmail = "recent@example.com";
        var oldToken = "old-token";
        var recentToken = "recent-token";

        _tracker.StoreLatestToken(oldEmail, oldToken, DateTime.UtcNow.AddHours(-25));
        _tracker.StoreLatestToken(recentEmail, recentToken, DateTime.UtcNow);

        // Act
        _tracker.CleanupOldEntries();

        // Assert
        _tracker.ValidateToken(oldEmail, "any").Should().BeTrue(); // Cleaned up
        _tracker.ValidateToken(recentEmail, recentToken).Should().BeTrue(); // Still valid
    }

    [Fact]
    public void CleanupOldEntries_WithNoEntries_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _tracker.CleanupOldEntries();
        act.Should().NotThrow();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ValidateToken_WithSameTokenDifferentCase_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var token = "CaseSensitiveToken";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, token, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, "casesensitivetoken");

        // Assert - tokens are case-sensitive (hashed)
        result.Should().BeFalse();
    }

    [Fact]
    public void StoreLatestToken_WithEmptyToken_ShouldHandle()
    {
        // Arrange
        var email = "test@example.com";
        var emptyToken = "";
        var timestamp = DateTime.UtcNow;

        // Act
        _tracker.StoreLatestToken(email, emptyToken, timestamp);

        // Assert
        _tracker.ValidateToken(email, emptyToken).Should().BeTrue();
    }

    [Fact]
    public void StoreLatestToken_WithVeryLongToken_ShouldHandle()
    {
        // Arrange
        var email = "test@example.com";
        var longToken = new string('a', 10000);
        var timestamp = DateTime.UtcNow;

        // Act & Assert
        var act = () => _tracker.StoreLatestToken(email, longToken, timestamp);
        act.Should().NotThrow();
        _tracker.ValidateToken(email, longToken).Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithSpecialCharactersInToken_ShouldValidate()
    {
        // Arrange
        var email = "test@example.com";
        var specialToken = "token+with/special=chars&symbols#$%";
        var timestamp = DateTime.UtcNow;
        _tracker.StoreLatestToken(email, specialToken, timestamp);

        // Act
        var result = _tracker.ValidateToken(email, specialToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - run multiple concurrent operations
        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                var email = $"user{index}@example.com";
                var token = $"token-{index}";
                _tracker.StoreLatestToken(email, token, DateTime.UtcNow);
                _tracker.ValidateToken(email, token);
                _tracker.ClearToken(email);
            }));
        }

        // Assert - should complete without exceptions
        var act = () => Task.WaitAll(tasks.ToArray());
        act.Should().NotThrow();
    }

    #endregion
}
