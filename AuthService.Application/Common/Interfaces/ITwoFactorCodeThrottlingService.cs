namespace AuthService.Application.Common.Interfaces;

public interface ITwoFactorCodeThrottlingService
{
    /// <summary>
    /// Checks if 2FA code resend is allowed for the given email
    /// Cooldown: 60 seconds
    /// Max attempts: 5 per day
    /// Code validity: 1 hour
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
    /// Store the latest code with its timestamp
    /// Only the latest code will be valid for verification
    /// </summary>
    void StoreCode(string email, string code, DateTime timestamp);

    /// <summary>
    /// Validate that a code is the latest one and hasn't expired (1 hour)
    /// </summary>
    bool ValidateCode(string email, string code);
}
