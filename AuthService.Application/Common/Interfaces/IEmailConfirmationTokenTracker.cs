namespace AuthService.Application.Common.Interfaces;

/// <summary>
/// Tracks the latest email confirmation token for each user
/// Ensures only the most recent confirmation email works
/// </summary>
public interface IEmailConfirmationTokenTracker
{
    /// <summary>
    /// Store the latest token (as hash) with its timestamp for a user
    /// Only this token will be valid for confirmation
    /// </summary>
    void StoreLatestToken(string email, string token, DateTime tokenTimestamp);
    
    /// <summary>
    /// Validate that a token is the latest one and hasn't expired (1 hour)
    /// </summary>
    bool ValidateToken(string email, string token);
    
    /// <summary>
    /// Clear token tracking for a user (after successful confirmation)
    /// </summary>
    void ClearToken(string email);
    
    /// <summary>
    /// Cleanup old entries
    /// </summary>
    void CleanupOldEntries();
}
