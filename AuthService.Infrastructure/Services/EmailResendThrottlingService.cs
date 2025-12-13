using System.Collections.Concurrent;
using System.Linq;
using AuthService.Application.Common.Interfaces;

namespace AuthService.Infrastructure.Services;

public class EmailResendThrottlingService : IEmailResendThrottlingService
{
    private readonly ConcurrentDictionary<string, ResendAttemptInfo> _resendAttempts = new();
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(60); // 60 seconds cooldown
    private readonly int _maxAttemptsPerDay = 5; // Maximum 5 attempts in 24 hours
    private readonly TimeSpan _dailyWindow = TimeSpan.FromHours(24);

    public class ResendAttemptInfo
    {
        public DateTime LastResendTime { get; set; }
        public List<DateTime> Attempts { get; set; } = new();
    }

    /// <summary>
    /// Checks if resend is allowed for the given email
    /// </summary>
    public (bool Allowed, string? Message, TimeSpan? RemainingCooldown) CanResend(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        
        if (!_resendAttempts.TryGetValue(normalizedEmail, out var attemptInfo))
        {
            // First attempt - allowed
            return (true, null, null);
        }

        var now = DateTime.UtcNow;

        // Check cooldown period (60 seconds between resends)
        var timeSinceLastResend = now - attemptInfo.LastResendTime;
        if (timeSinceLastResend < _cooldownPeriod)
        {
            var remainingTime = _cooldownPeriod - timeSinceLastResend;
            return (false, $"Please wait {remainingTime.TotalSeconds:F0} seconds before requesting another email.", remainingTime);
        }

        // Clean up old attempts (older than 24 hours)
        attemptInfo.Attempts.RemoveAll(a => now - a > _dailyWindow);

        // Check daily limit
        if (attemptInfo.Attempts.Count >= _maxAttemptsPerDay)
        {
            var oldestAttempt = attemptInfo.Attempts.Min();
            var timeUntilReset = _dailyWindow - (now - oldestAttempt);
            return (false, $"You have reached the maximum number of resend attempts ({_maxAttemptsPerDay}) in 24 hours. Please try again later.", timeUntilReset);
        }

        return (true, null, null);
    }

    /// <summary>
    /// Records a resend attempt
    /// </summary>
    public void RecordResendAttempt(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var now = DateTime.UtcNow;

        _resendAttempts.AddOrUpdate(
            normalizedEmail,
            _ => new ResendAttemptInfo 
            { 
                LastResendTime = now,
                Attempts = new List<DateTime> { now }
            },
            (_, existing) =>
            {
                existing.LastResendTime = now;
                existing.Attempts.Add(now);
                // Clean up old attempts
                existing.Attempts.RemoveAll(a => now - a > _dailyWindow);
                return existing;
            });
    }

    /// <summary>
    /// Clears resend attempts for an email (used when account is confirmed)
    /// </summary>
    public void ClearAttempts(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        _resendAttempts.TryRemove(normalizedEmail, out _);
    }

    /// <summary>
    /// Cleanup old entries periodically
    /// </summary>
    public void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _resendAttempts
            .Where(kvp => now - kvp.Value.LastResendTime > _dailyWindow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _resendAttempts.TryRemove(key, out _);
        }
    }
}
