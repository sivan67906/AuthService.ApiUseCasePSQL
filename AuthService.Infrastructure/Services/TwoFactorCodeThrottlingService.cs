using System.Collections.Concurrent;
using System.Linq;
using AuthService.Application.Common.Interfaces;

namespace AuthService.Infrastructure.Services;

public class TwoFactorCodeThrottlingService : ITwoFactorCodeThrottlingService
{
    private readonly ConcurrentDictionary<string, ResendAttemptInfo> _resendAttempts = new();
    private readonly ConcurrentDictionary<string, DateTime> _latestCodeTimestamps = new();
    
    // Configuration for 2FA
    private const int CooldownSeconds = 60;        // 60 seconds between resends
    private const int MaxAttemptsPerDay = 5;       // Maximum 5 attempts per day
    private const int CodeValidityMinutes = 5;     // Code valid for 5 minutes
    
    private class ResendAttemptInfo
    {
        public List<DateTime> Attempts { get; set; } = new();
        public DateTime LastAttempt { get; set; }
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
        _latestCodeTimestamps.TryRemove(email, out _);
    }

    public void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _resendAttempts
            .Where(kvp => (now - kvp.Value.LastAttempt).TotalHours > 24)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _resendAttempts.TryRemove(key, out _);
        }
        
        // Also cleanup old timestamp entries
        var timestampKeysToRemove = _latestCodeTimestamps
            .Where(kvp => (now - kvp.Value).TotalHours > 24)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in timestampKeysToRemove)
        {
            _latestCodeTimestamps.TryRemove(key, out _);
        }
    }

    public void StoreLatestCodeTimestamp(string email, DateTime timestamp)
    {
        _latestCodeTimestamps[email] = timestamp;
    }

    public bool IsCodeTimestampValid(string email, DateTime codeTimestamp)
    {
        // Check if code is still within 5-minute validity window
        var now = DateTime.UtcNow;
        var codeAge = now - codeTimestamp;
        
        if (codeAge.TotalMinutes > CodeValidityMinutes)
        {
            return false; // Code expired
        }
        
        // Check if this code has been superseded by a newer one
        if (_latestCodeTimestamps.TryGetValue(email, out var latestTimestamp))
        {
            return codeTimestamp >= latestTimestamp;
        }
        
        return true;
    }
}
