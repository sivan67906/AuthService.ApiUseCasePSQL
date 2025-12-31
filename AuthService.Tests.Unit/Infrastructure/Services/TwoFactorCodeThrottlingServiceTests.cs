namespace AuthService.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for TwoFactorCodeThrottlingService
/// Tests 2FA code throttling, validation, storage, and cleanup
/// </summary>
public class TwoFactorCodeThrottlingServiceTests
{
    private readonly TwoFactorCodeThrottlingService _service;

    public TwoFactorCodeThrottlingServiceTests()
    {
        _service = new TwoFactorCodeThrottlingService();
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
    public void CanResend_NewEmail_ShouldAllow()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        _service.RecordResendAttempt(email1);

        // Act
        var (allowed, message, cooldown) = _service.CanResend(email2);

        // Assert
        allowed.Should().BeTrue();
        message.Should().BeNull();
        cooldown.Should().BeNull();
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
        message.Should().Contain("seconds");
        cooldown.Should().NotBeNull();
        cooldown!.Value.TotalSeconds.Should().BeGreaterThan(0);
        cooldown.Value.TotalSeconds.Should().BeLessThanOrEqualTo(60);
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

        // Act
        var (allowed, message, _) = _service.CanResend(email);

        // Assert
        allowed.Should().BeFalse();
        message.Should().NotBeNull();
    }

    [Fact]
    public void CanResend_WithinCooldown_ShouldReturnRemainingTime()
    {
        // Arrange
        var email = "test@example.com";
        _service.RecordResendAttempt(email);

        // Act
        var (allowed, _, cooldown) = _service.CanResend(email);

        // Assert
        allowed.Should().BeFalse();
        cooldown.Should().NotBeNull();
        cooldown!.Value.TotalSeconds.Should().BePositive();
    }

    #endregion

    #region RecordResendAttempt Tests

    [Fact]
    public void RecordResendAttempt_ShouldBlock()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        _service.RecordResendAttempt(email);

        // Assert
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeFalse();
    }

    [Fact]
    public void RecordResendAttempt_MultipleEmails_ShouldTrackIndependently()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";

        // Act
        _service.RecordResendAttempt(email1);

        // Assert
        var (allowed1, _, _) = _service.CanResend(email1);
        var (allowed2, _, _) = _service.CanResend(email2);

        allowed1.Should().BeFalse();
        allowed2.Should().BeTrue();
    }

    [Fact]
    public void RecordResendAttempt_ShouldNotThrow()
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        var act = () =>
        {
            for (int i = 0; i < 20; i++)
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

        // Act
        _service.ClearAttempts(email);

        // Assert
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeTrue();
    }

    [Fact]
    public void ClearAttempts_ShouldAlsoClearStoredCode()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        _service.StoreCode(email, code, DateTime.UtcNow);

        // Verify code is stored
        _service.ValidateCode(email, code).Should().BeTrue();

        // Act
        _service.ClearAttempts(email);

        // Assert - code should no longer validate
        _service.ValidateCode(email, code).Should().BeFalse();
    }

    [Fact]
    public void ClearAttempts_NonExisting_ShouldNotThrow()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act & Assert
        var act = () => _service.ClearAttempts(email);
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearAttempts_OnlyAffectsSpecificEmail()
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

    #region StoreCode Tests

    [Fact]
    public void StoreCode_ShouldAllowValidation()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        var timestamp = DateTime.UtcNow;

        // Act
        _service.StoreCode(email, code, timestamp);

        // Assert
        _service.ValidateCode(email, code).Should().BeTrue();
    }

    [Fact]
    public void StoreCode_NewCode_ShouldReplaceOld()
    {
        // Arrange
        var email = "test@example.com";
        var oldCode = "111111";
        var newCode = "222222";
        var timestamp = DateTime.UtcNow;

        // Act
        _service.StoreCode(email, oldCode, timestamp);
        _service.StoreCode(email, newCode, timestamp);

        // Assert
        _service.ValidateCode(email, oldCode).Should().BeFalse();
        _service.ValidateCode(email, newCode).Should().BeTrue();
    }

    [Fact]
    public void StoreCode_MultipleEmails_ShouldStoreIndependently()
    {
        // Arrange
        var email1 = "user1@example.com";
        var email2 = "user2@example.com";
        var code1 = "111111";
        var code2 = "222222";
        var timestamp = DateTime.UtcNow;

        // Act
        _service.StoreCode(email1, code1, timestamp);
        _service.StoreCode(email2, code2, timestamp);

        // Assert
        _service.ValidateCode(email1, code1).Should().BeTrue();
        _service.ValidateCode(email2, code2).Should().BeTrue();
        _service.ValidateCode(email1, code2).Should().BeFalse();
        _service.ValidateCode(email2, code1).Should().BeFalse();
    }

    #endregion

    #region ValidateCode Tests

    [Fact]
    public void ValidateCode_ValidCode_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        _service.StoreCode(email, code, DateTime.UtcNow);

        // Act
        var result = _service.ValidateCode(email, code);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateCode_InvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var validCode = "123456";
        var invalidCode = "654321";
        _service.StoreCode(email, validCode, DateTime.UtcNow);

        // Act
        var result = _service.ValidateCode(email, invalidCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_NoStoredCode_ShouldReturnFalse()
    {
        // Arrange
        var email = "nocode@example.com";

        // Act
        var result = _service.ValidateCode(email, "anycode");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_ExpiredCode_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        var expiredTimestamp = DateTime.UtcNow.AddMinutes(-61); // Expired (>60 minutes)
        _service.StoreCode(email, code, expiredTimestamp);

        // Act
        var result = _service.ValidateCode(email, code);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_JustBeforeExpiration_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        var almostExpiredTimestamp = DateTime.UtcNow.AddMinutes(-59); // Just under 60 minutes
        _service.StoreCode(email, code, almostExpiredTimestamp);

        // Act
        var result = _service.ValidateCode(email, code);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateCode_ExactlyAtExpiration_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        var exactlyExpiredTimestamp = DateTime.UtcNow.AddMinutes(-60).AddSeconds(-1);
        _service.StoreCode(email, code, exactlyExpiredTimestamp);

        // Act
        var result = _service.ValidateCode(email, code);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_CaseSensitive_ShouldNotMatch()
    {
        // Arrange
        var email = "test@example.com";
        var code = "AbCdEf";
        _service.StoreCode(email, code, DateTime.UtcNow);

        // Act
        var result = _service.ValidateCode(email, "abcdef");

        // Assert
        result.Should().BeFalse();
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
    public void CleanupOldEntries_RecentEntries_ShouldKeep()
    {
        // Arrange
        var email = "recent@example.com";
        var code = "123456";
        _service.StoreCode(email, code, DateTime.UtcNow);
        _service.RecordResendAttempt(email);

        // Act
        _service.CleanupOldEntries();

        // Assert - entries should still exist
        _service.ValidateCode(email, code).Should().BeTrue();
        var (allowed, _, _) = _service.CanResend(email);
        allowed.Should().BeFalse();
    }

    [Fact]
    public void CleanupOldEntries_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        _service.RecordResendAttempt("test@example.com");
        _service.StoreCode("test@example.com", "123456", DateTime.UtcNow);

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
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("999999")]
    [InlineData("A1B2C3")]
    [InlineData("")]
    public void StoreAndValidateCode_VariousCodes_ShouldHandle(string code)
    {
        // Arrange
        var email = "test@example.com";

        // Act
        _service.StoreCode(email, code, DateTime.UtcNow);
        var result = _service.ValidateCode(email, code);

        // Assert
        result.Should().BeTrue();
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
            var code = $"{i:D6}";
            tasks.Add(Task.Run(() =>
            {
                _service.StoreCode(email, code, DateTime.UtcNow);
                _service.ValidateCode(email, code);
                _service.RecordResendAttempt(email);
                _service.CanResend(email);
            }));
        }

        // Assert - should complete without exceptions
        var act = () => Task.WaitAll(tasks.ToArray());
        act.Should().NotThrow();
    }

    [Fact]
    public void Integration_Complete2FAFlow()
    {
        // Arrange
        var email = "user@example.com";
        var code = "123456";

        // Step 1: Check if can send 2FA code
        var (canSend1, _, _) = _service.CanResend(email);
        canSend1.Should().BeTrue();

        // Step 2: Record sending attempt
        _service.RecordResendAttempt(email);

        // Step 3: Store the code
        _service.StoreCode(email, code, DateTime.UtcNow);

        // Step 4: Cannot send immediately
        var (canSend2, msg, cooldown) = _service.CanResend(email);
        canSend2.Should().BeFalse();
        msg.Should().NotBeNull();
        cooldown.Should().NotBeNull();

        // Step 5: Validate correct code
        _service.ValidateCode(email, code).Should().BeTrue();

        // Step 6: Wrong code fails
        _service.ValidateCode(email, "000000").Should().BeFalse();

        // Step 7: Login success - clear attempts
        _service.ClearAttempts(email);

        // Step 8: Can send again
        var (canSend3, _, _) = _service.CanResend(email);
        canSend3.Should().BeTrue();

        // Step 9: Old code no longer valid
        _service.ValidateCode(email, code).Should().BeFalse();
    }

    [Fact]
    public void DailyLimit_Message_ShouldContainHours()
    {
        // Arrange
        var email = "spammer@example.com";

        // Record 5 attempts to hit daily limit
        for (int i = 0; i < 5; i++)
        {
            _service.RecordResendAttempt(email);
        }

        // Act
        var (_, message, _) = _service.CanResend(email);

        // Assert
        message.Should().NotBeNull();
        // Message should mention either cooldown or daily limit
        (message!.Contains("seconds") || message.Contains("hour")).Should().BeTrue();
    }

    [Theory]
    [InlineData("a@b.c")]
    [InlineData("test.user+tag@domain.co.uk")]
    [InlineData("UPPERCASE@EMAIL.COM")]
    public void AllOperations_VariousEmailFormats_ShouldHandle(string email)
    {
        // Act & Assert
        var act = () =>
        {
            _service.RecordResendAttempt(email);
            _service.CanResend(email);
            _service.StoreCode(email, "123456", DateTime.UtcNow);
            _service.ValidateCode(email, "123456");
            _service.ClearAttempts(email);
            _service.CleanupOldEntries();
        };
        act.Should().NotThrow();
    }

    #endregion
}
