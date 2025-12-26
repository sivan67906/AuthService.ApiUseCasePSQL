using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Infrastructure.Services;

public class TwoFactorCodeThrottlingService : ITwoFactorCodeThrottlingService
{
    private readonly ConcurrentDictionary<string, ResendAttemptInfo> _resendAttempts = new();
    private readonly ConcurrentDictionary<string, StoredCodeInfo> _storedCodes = new();

    // Configuration for 2FA
    private const int CooldownSeconds = 60;        // 60 seconds between resends
    private const int MaxAttemptsPerDay = 5;       // Maximum 5 attempts per day
    private const int CodeValidityMinutes = 60;    // Code valid for 1 hour (60 minutes)

    private class ResendAttemptInfo
    {
        public List<DateTime> Attempts { get; set; } = new();
        public DateTime LastAttempt { get; set; }
    }

    private class StoredCodeInfo
    {
        public string CodeHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public (bool Allowed, string? Message, TimeSpan? RemainingCooldown) CanResend(string email)
    {
        var now = DateTime.UtcNow;

        if (_resendAttempts.TryGetValue(email, out var attemptInfo))
        {
            // Check cooldown (60 seconds)
            var timeSinceLastAttempt = now - attemptInfo.LastAttempt;
            if (timeSinceLastAttempt.TotalSeconds < CooldownSeconds)
            {
                var remainingSeconds = CooldownSeconds - (int)timeSinceLastAttempt.TotalSeconds;
                return (false, $"Please wait {remainingSeconds} seconds before requesting another code.",
                    TimeSpan.FromSeconds(remainingSeconds));
            }

            // Clean up attempts older than 24 hours
            attemptInfo.Attempts = attemptInfo.Attempts
                .Where(a => (now - a).TotalHours < 24)
                .ToList();

            // Check daily limit (5 attempts)
            if (attemptInfo.Attempts.Count >= MaxAttemptsPerDay)
            {
                var oldestAttempt = attemptInfo.Attempts.Min();
                var resetTime = oldestAttempt.AddHours(24);
                var hoursUntilReset = (int)Math.Ceiling((resetTime - now).TotalHours);

                return (false, $"Maximum {MaxAttemptsPerDay} code requests reached for today. Try again in {hoursUntilReset} hour(s).",
                    resetTime - now);
            }
        }

        return (true, null, null);
    }

    public void RecordResendAttempt(string email)
    {
        var now = DateTime.UtcNow;

        _resendAttempts.AddOrUpdate(
            email,
            new ResendAttemptInfo
            {
                Attempts = new List<DateTime> { now },
                LastAttempt = now
            },
            (key, existing) =>
            {
                existing.Attempts.Add(now);
                existing.LastAttempt = now;
                return existing;
            });
    }

    public void ClearAttempts(string email)
    {
        _resendAttempts.TryRemove(email, out _);
        _storedCodes.TryRemove(email, out _);
    }

    public void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;

        // Cleanup resend attempts
        var keysToRemove = _resendAttempts
            .Where(kvp => (now - kvp.Value.LastAttempt).TotalHours > 24)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _resendAttempts.TryRemove(key, out _);
        }

        // Cleanup stored codes older than 1 hour
        var codeKeysToRemove = _storedCodes
            .Where(kvp => (now - kvp.Value.Timestamp).TotalMinutes > CodeValidityMinutes)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in codeKeysToRemove)
        {
            _storedCodes.TryRemove(key, out _);
        }
    }

    public void StoreCode(string email, string code, DateTime timestamp)
    {
        var codeHash = HashCode(code);
        _storedCodes[email] = new StoredCodeInfo
        {
            CodeHash = codeHash,
            Timestamp = timestamp
        };
    }

    public bool ValidateCode(string email, string code)
    {
        // Check if we have a stored code for this email
        if (!_storedCodes.TryGetValue(email, out var storedCodeInfo))
        {
            // No stored code - this shouldn't happen in normal flow
            return false;
        }

        // Check if code has expired (1 hour)
        var now = DateTime.UtcNow;
        var codeAge = now - storedCodeInfo.Timestamp;
        if (codeAge.TotalMinutes > CodeValidityMinutes)
        {
            return false; // Code expired
        }

        // Check if the provided code matches the stored code
        var providedCodeHash = HashCode(code);
        return providedCodeHash == storedCodeInfo.CodeHash;
    }

    private static string HashCode(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
