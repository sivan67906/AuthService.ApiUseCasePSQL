using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.EmailConfirmation;
public class SendEmailConfirmationCommandHandler : IRequestHandler<SendEmailConfirmationCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;
    private readonly IEmailResendThrottlingService _throttlingService;
    private readonly IEmailConfirmationTokenTracker _tokenTracker;
    
    public SendEmailConfirmationCommandHandler(
        UserManager<ApplicationUser> userManager, 
        IEmailService email,
        IEmailResendThrottlingService throttlingService,
        IEmailConfirmationTokenTracker tokenTracker)
    {
        _userManager = userManager;
        _email = email;
        _throttlingService = throttlingService;
        _tokenTracker = tokenTracker;
    }
    
    public async Task<bool> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // don't reveal user existence
            return true;
        }
        
        // Issue #2: Check if account is already confirmed
        if (user.EmailConfirmed)
        {
            // Clear throttling attempts since account is confirmed
            _throttlingService.ClearAttempts(request.Email);
            throw new InvalidOperationException("Your account is already confirmed. Please login.");
        }
        
        // Issue #2: Check throttling limits
        var (allowed, message, remainingTime) = _throttlingService.CanResend(request.Email);
        if (!allowed)
        {
            throw new InvalidOperationException(message ?? "Please wait before requesting another email.");
        }
        
        // Issue #1 FIX: DO NOT update security stamp here!
        // UpdateSecurityStampAsync invalidates ALL previous Identity tokens including email confirmation.
        // Instead, we use EmailConfirmationTokenTracker to track which token is the latest.
        // The tracker rejects old tokens based on timestamp comparison, not security stamp invalidation.
        
        // Issue #3: Generate token with 1-hour expiry
        var standardToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var tokenTimestamp = DateTime.UtcNow;
        var expiryTime = tokenTimestamp.AddHours(1);
        
        // Issue #4: Track this as the latest token for this user
        // Only this token (and any future ones) will be accepted
        _tokenTracker.StoreLatestToken(request.Email, tokenTimestamp);
        
        // Create custom token with timestamp: userId|tokenTimestamp|expiryTimestamp|standardToken
        var customToken = $"{user.Id}|{tokenTimestamp:O}|{expiryTime:O}|{standardToken}";
        var encodedToken = WebUtility.UrlEncode(customToken);
        
        var callbackUrl = $"{request.CallbackBaseUrl}?email={WebUtility.UrlEncode(user.Email)}&token={encodedToken}";
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = user.Email ?? "User";
        }
        
        var baseDir = AppContext.BaseDirectory;
        var templatePath = Path.Combine(baseDir, "EmailTemplates", "confirmmail-verify-email.html");
        var html = await File.ReadAllTextAsync(templatePath, cancellationToken);
        
        // Replace placeholders
        html = html.Replace("https://pixinvent.com?verification_url", callbackUrl);
        html = html.Replace("John Doe", fullName);
        html = html.Replace("support@example.com", "support@" + (user.Email?.Split('@')[1] ?? "example.com"));
        
        await _email.SendAsync(user.Email!, "Confirm your email", html, cancellationToken);
        
        // Issue #2: Record the resend attempt
        _throttlingService.RecordResendAttempt(request.Email);
        
        return true;
    }
}
