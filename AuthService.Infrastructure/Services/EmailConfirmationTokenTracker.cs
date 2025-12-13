using System.Collections.Concurrent;
using System.Linq;
using AuthService.Application.Common.Interfaces;

namespace AuthService.Infrastructure.Services;

public class EmailConfirmationTokenTracker : IEmailConfirmationTokenTracker
{
    private readonly ConcurrentDictionary<string, DateTime> _latestTokens = new();

    public void StoreLatestToken(string email, DateTime tokenTimestamp)
    {
        _latestTokens[email.ToLowerInvariant()] = tokenTimestamp;
    }

    public bool IsLatestToken(string email, DateTime tokenTimestamp)
    {
        var key = email.ToLowerInvariant();
        
        if (!_latestTokens.TryGetValue(key, out var latestTimestamp))
        {
            // No tracking yet - allow the token (backward compatibility)
            return true;
        }

        // Only allow if this token timestamp matches or is newer than the latest
        // Allow small tolerance (1 second) for timing issues
        return (tokenTimestamp - latestTimestamp).TotalSeconds >= -1;
    }

    public void ClearToken(string email)
    {
        _latestTokens.TryRemove(email.ToLowerInvariant(), out _);
    }

    public void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _latestTokens
            .Where(kvp => (now - kvp.Value).TotalHours > 24)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _latestTokens.TryRemove(key, out _);
        }
    }
}

