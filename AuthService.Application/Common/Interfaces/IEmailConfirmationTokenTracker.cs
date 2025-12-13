namespace AuthService.Application.Common.Interfaces;

/// <summary>
/// Tracks the latest email confirmation token for each user
/// Ensures only the most recent confirmation email works
/// </summary>
public interface IEmailConfirmationTokenTracker
{
    /// <summary>
    /// Store the latest token timestamp for a user
    /// </summary>
    void StoreLatestToken(string email, DateTime tokenTimestamp);
    
    /// <summary>
    /// Check if a token timestamp is the latest one for this user
    /// </summary>
    bool IsLatestToken(string email, DateTime tokenTimestamp);
    
    /// <summary>
    /// Clear token tracking for a user (after successful confirmation)
    /// </summary>
    void ClearToken(string email);
    
    /// <summary>
    /// Cleanup old entries
    /// </summary>
    void CleanupOldEntries();
}
