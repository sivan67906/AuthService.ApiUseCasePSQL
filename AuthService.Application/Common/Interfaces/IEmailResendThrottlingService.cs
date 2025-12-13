namespace AuthService.Application.Common.Interfaces;

public interface IEmailResendThrottlingService
{
    /// <summary>
    /// Checks if resend is allowed for the given email
    /// </summary>
    (bool Allowed, string? Message, TimeSpan? RemainingCooldown) CanResend(string email);

    /// <summary>
    /// Records a resend attempt
    /// </summary>
    void RecordResendAttempt(string email);

    /// <summary>
    /// Clears resend attempts for an email (used when account is confirmed)
    /// </summary>
    void ClearAttempts(string email);

    /// <summary>
    /// Cleanup old entries periodically
    /// </summary>
    void CleanupOldEntries();
}
