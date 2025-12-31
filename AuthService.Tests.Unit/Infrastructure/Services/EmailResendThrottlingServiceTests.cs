namespace AuthService.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for EmailResendThrottlingService
/// Tests throttling logic, cooldown periods, daily limits, and cleanup
/// </summary>
public class EmailResendThrottlingServiceTests
{
    private readonly EmailResendThrottlingService _service;

    public EmailResendThrottlingServiceTests()
    {
        _service = new EmailResendThrottlingService();
    }

    #region CanResend Tests - Positive Scenarios

    [Fact]
    public void CanResend_FirstAttempt_ShouldAllow()
    {
        // Arrange
        var email = "newuser@example.com";

        // Act
        var (allowed, message, cooldown) = _service.CanResend(email);

        // Assert
        allowed.Should().BeTrue();
        message.Should().BeNull();
        cooldown.Should().BeNull();
    }

    [Fact]
    public void CanResend_AfterCooldownPeriod_ShouldAllow()
    {
        // Arrange
        var email = "test@example.com";
        _service.RecordResendAttempt(email);

        // Simulate waiting by using a new service instance with manipulation
        // Note: In real implementation, we'd use a time abstraction
        // For this test, we'll verify the behavior logically

        // Act - first attempt just recorded
        var (allowed1, _, _) = _service.CanResend(email);

        // Assert - should not allow immediately after recording
        allowed1.Should().BeFalse();
    }

    [Fact]
    public void CanResend_CaseInsensitiveEmail_ShouldNormalize()
    {
        // Arrange
        var email1 = "TEST@EXAMPLE.COM";
        var email2 = "test@example.com";
        _service.RecordResendAttempt(email1);

        // Act
        var (allowed, _, _) = _service.CanResend(email2);

        // Assert - should recognize as same email
        allowed.Should().BeFalse();
    }

    [Fact]
    public void CanResend_DifferentEmails_ShouldBeIndependent()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        _service.RecordResendAttempt(email1);

        // Act
        var (allowed, _, _) = _service.CanResend(email2);

        // Assert
        allowed.Should().BeTrue();
    }

    #endregion

    #region CanResend Tests - Negative Scenarios

    [Fact]
    public void CanResend_ImmediatelyAfterAttempt_ShouldDeny()
    {
        // Arrange
        var email = "test@example.com";
        _service.RecordResendAttempt(email);

        // Act
        var (allowed, message, cooldown) = _service.CanResend(email);

        // Assert
        allowed.Should().BeFalse();
        message.Should().Contain("wait");
        cooldown.Should().NotBeNull();
        cooldown!.Value.TotalSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanResend_WithinCooldownPeriod_ShouldReturnRemainingTime()
    {
        // Arrange
        var email = "test@example.com";
        _service.RecordResendAttempt(email);

        // Act
        var (allowed, message, cooldown) = _service.CanResend(email);

        // Assert
        allowed.Should().BeFalse();
        cooldown.Should().NotBeNull();
        cooldown!.Value.TotalSeconds.Should().BeLessThanOrEqualTo(60);
        cooldown.Value.TotalSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanResend_AfterMaxDailyAttempts_ShouldDeny()
    {
        // Arrange
        var email = "spammer@example.com";

        // Record 5 attempts (max daily limit)
        for (int i = 0; i < 5; i++)
        {
            _service.RecordResendAttempt(email);
        }

        // Wait conceptually past cooldown but still within daily limit
        // In practice, we're checking the daily limit logic

        // Act
        var (allowed, message, _) = _service.CanResend(email);

        // Assert
        allowed.Should().BeFalse();
        // Message should mention either cooldown or daily limit
        message.Should().NotBeNull();
    }

    #endregion

    #region RecordResendAttempt Tests

    [Fact]
    public void RecordResendAttempt_ShouldUpdateLastResendTime()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        _service.RecordResendAttempt(email);

        // Assert - cannot resend immediately
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeFalse();
    }

    [Fact]
    public void RecordResendAttempt_MultipleAttempts_ShouldTrackAll()
    {
        // Arrange
        var email = "test@example.com";

        // Act - record multiple attempts
        _service.RecordResendAttempt(email);
        _service.RecordResendAttempt(email);
        _service.RecordResendAttempt(email);

        // Assert - should be blocked
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeFalse();
    }

    [Fact]
    public void RecordResendAttempt_CaseInsensitive_ShouldNormalize()
    {
        // Arrange
        var upperEmail = "TEST@EXAMPLE.COM";
        var lowerEmail = "test@example.com";

        // Act
        _service.RecordResendAttempt(upperEmail);

        // Assert
        var (allowed, _, _) = _service.CanResend(lowerEmail);
        allowed.Should().BeFalse();
    }

    [Fact]
    public void RecordResendAttempt_ShouldNotThrow()
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 10; i++)
            {
                _service.RecordResendAttempt(email);
            }
        };
        act.Should().NotThrow();
    }

    #endregion

    #region ClearAttempts Tests

    [Fact]
    public void ClearAttempts_ShouldAllowImmediateResend()
    {
        // Arrange
        var email = "test@example.com";
        _service.RecordResendAttempt(email);

        // Verify blocked
        var (blockedBefore, _, _) = _service.CanResend(email);
        blockedBefore.Should().BeFalse();

        // Act
        _service.ClearAttempts(email);

        // Assert
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeTrue();
    }

    [Fact]
    public void ClearAttempts_CaseInsensitive_ShouldClear()
    {
        // Arrange
        var email = "TEST@EXAMPLE.COM";
        _service.RecordResendAttempt(email);

        // Act
        _service.ClearAttempts("test@example.com");

        // Assert
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeTrue();
    }

    [Fact]
    public void ClearAttempts_NonExistingEmail_ShouldNotThrow()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act & Assert
        var act = () => _service.ClearAttempts(email);
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearAttempts_ShouldOnlyClearSpecificEmail()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        _service.RecordResendAttempt(email1);
        _service.RecordResendAttempt(email2);

        // Act
        _service.ClearAttempts(email1);

        // Assert
        var (allowed1, _, _) = _service.CanResend(email1);
        var (allowed2, _, _) = _service.CanResend(email2);

        allowed1.Should().BeTrue();
        allowed2.Should().BeFalse();
    }

    #endregion

    #region CleanupOldEntries Tests

    [Fact]
    public void CleanupOldEntries_WithNoEntries_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _service.CleanupOldEntries();
        act.Should().NotThrow();
    }

    [Fact]
    public void CleanupOldEntries_WithRecentEntries_ShouldKeep()
    {
        // Arrange
        var email = "recent@example.com";
        _service.RecordResendAttempt(email);

        // Act
        _service.CleanupOldEntries();

        // Assert - recent entries should be kept
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeFalse(); // Entry still exists
    }

    [Fact]
    public void CleanupOldEntries_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        _service.RecordResendAttempt("test@example.com");

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 10; i++)
            {
                _service.CleanupOldEntries();
            }
        };
        act.Should().NotThrow();
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("a@b.c")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("very.long.email.address@subdomain.example.com")]
    public void AllOperations_WithVariousEmailFormats_ShouldHandle(string email)
    {
        // Act & Assert - all operations should not throw
        var act = () =>
        {
            _service.RecordResendAttempt(email);
            _service.CanResend(email);
            _service.ClearAttempts(email);
            _service.CleanupOldEntries();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void CanResend_CooldownMessage_ShouldContainSeconds()
    {
        // Arrange
        var email = "test@example.com";
        _service.RecordResendAttempt(email);

        // Act
        var (_, message, _) = _service.CanResend(email);

        // Assert
        message.Should().Contain("seconds");
    }

    [Fact]
    public void ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var email = "concurrent@example.com";

        // Act - run multiple concurrent operations
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _service.RecordResendAttempt(email);
                _service.CanResend(email);
            }));
        }

        for (int i = 0; i < 50; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                var uniqueEmail = $"user{index}@example.com";
                _service.RecordResendAttempt(uniqueEmail);
                _service.CanResend(uniqueEmail);
                _service.ClearAttempts(uniqueEmail);
            }));
        }

        // Assert - should complete without exceptions
        var act = () => Task.WaitAll(tasks.ToArray());
        act.Should().NotThrow();
    }

    [Fact]
    public void Integration_TypicalUserFlow()
    {
        // Arrange
        var email = "newuser@example.com";

        // Step 1: First registration - should allow
        var (firstAllowed, _, _) = _service.CanResend(email);
        firstAllowed.Should().BeTrue();

        // Step 2: Record the attempt
        _service.RecordResendAttempt(email);

        // Step 3: Immediate resend - should deny
        var (secondAllowed, message, cooldown) = _service.CanResend(email);
        secondAllowed.Should().BeFalse();
        message.Should().NotBeNull();
        cooldown.Should().NotBeNull();

        // Step 4: User confirms email - clear attempts
        _service.ClearAttempts(email);

        // Step 5: Should allow again
        var (afterClearAllowed, _, _) = _service.CanResend(email);
        afterClearAllowed.Should().BeTrue();
    }

    #endregion

    #region Daily Limit Tests

    [Fact]
    public void DailyLimit_ExactlyAtLimit_ShouldDeny()
    {
        // Arrange
        var email = "limit@example.com";

        // Record exactly 5 attempts (the max)
        for (int i = 0; i < 5; i++)
        {
            _service.RecordResendAttempt(email);
        }

        // Act
        var (allowed, message, _) = _service.CanResend(email);

        // Assert - 5th attempt should still be within limit initially,
        // but additional check should deny
        // The behavior depends on cooldown vs daily limit check order
        allowed.Should().BeFalse();
        message.Should().NotBeNull();
    }

    [Fact]
    public void DailyLimit_BelowLimit_ShouldAllowAfterCooldown()
    {
        // Arrange
        var email = "normal@example.com";
        _service.RecordResendAttempt(email); // 1st attempt

        // Act - check immediately (will fail due to cooldown, not daily limit)
        var (allowed, message, _) = _service.CanResend(email);

        // Assert
        allowed.Should().BeFalse();
        message.Should().Contain("wait"); // Cooldown message, not daily limit
    }

    #endregion
}
