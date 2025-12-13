namespace AuthService.Application.Common.Interfaces;

public interface ITwoFactorCodeThrottlingService
{
    /// <summary>
    /// Checks if 2FA code resend is allowed for the given email
    /// Cooldown: 60 seconds
    /// Max attempts: 5 per day
    /// Code validity: 5 minutes
    /// </summary>
    (bool Allowed, string? Message, TimeSpan? RemainingCooldown) CanResend(string email);

    /// <summary>
    /// Records a 2FA code send attempt
    /// </summary>
    void RecordResendAttempt(string email);

    /// <summary>
    /// Clears resend attempts for an email (used when user successfully logs in)
    /// </summary>
    void ClearAttempts(string email);

    /// <summary>
    /// Cleanup old entries periodically
    /// </summary>
    void CleanupOldEntries();
    
    /// <summary>
    /// Track the latest code timestamp to invalidate old codes
    /// </summary>
    void StoreLatestCodeTimestamp(string email, DateTime timestamp);
    
    /// <summary>
    /// Check if a code timestamp is still valid (not superseded by a newer code)
    /// </summary>
    bool IsCodeTimestampValid(string email, DateTime codeTimestamp);
}
