using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.Services;

public class EmailConfirmationTokenTracker : IEmailConfirmationTokenTracker
{
    private readonly ConcurrentDictionary<string, StoredTokenInfo> _latestTokens = new();

    private class StoredTokenInfo
    {
        public string TokenHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public void StoreLatestToken(string email, string token, DateTime tokenTimestamp)
    {
        var tokenHash = HashToken(token);
        _latestTokens[email.ToLowerInvariant()] = new StoredTokenInfo
        {
            TokenHash = tokenHash,
            Timestamp = tokenTimestamp
        };
    }

    public bool ValidateToken(string email, string token)
    {
        var key = email.ToLowerInvariant();

        if (!_latestTokens.TryGetValue(key, out var storedInfo))
        {
            // No tracking yet - allow the token (backward compatibility)
            return true;
        }

        // Check if token has expired (1 hour)
        var now = DateTime.UtcNow;
        var tokenAge = now - storedInfo.Timestamp;
        if (tokenAge.TotalHours > 1)
        {
            return false; // Token expired
        }

        // Check if the provided token matches the stored token
        var providedTokenHash = HashToken(token);
        return providedTokenHash == storedInfo.TokenHash;
    }

    public void ClearToken(string email)
    {
        _latestTokens.TryRemove(email.ToLowerInvariant(), out _);
    }

    public void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _latestTokens
            .Where(kvp => (now - kvp.Value.Timestamp).TotalHours > 24)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _latestTokens.TryRemove(key, out _);
        }
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}

