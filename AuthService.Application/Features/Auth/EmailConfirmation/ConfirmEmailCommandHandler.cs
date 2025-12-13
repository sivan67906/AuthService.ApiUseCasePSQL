using Microsoft.AspNetCore.Identity;
using System.Net;
using AuthService.Application.Common.Interfaces;

namespace AuthService.Application.Features.Auth.EmailConfirmation;
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailConfirmationTokenTracker _tokenTracker;
    
    public ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailConfirmationTokenTracker tokenTracker)
    {
        _userManager = userManager;
        _tokenTracker = tokenTracker;
    }
    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("User not found.");
        
        // Parse and validate custom token
        // NOTE: Do NOT URL decode here - ASP.NET Core's [FromQuery] already decodes the token.
        // Double-decoding would convert '+' characters (common in Base64) to spaces, breaking the token.
        var tokenParts = request.Token.Split('|');
        
        if (tokenParts.Length == 4)
        {
            // New format: userId|tokenTimestamp|expiryTimestamp|standardToken
            var userId = tokenParts[0];
            var tokenTimestampString = tokenParts[1];
            var expiryString = tokenParts[2];
            var standardToken = tokenParts[3];
            
            // Validate user ID matches
            if (userId != user.Id.ToString())
            {
                throw new InvalidOperationException("Invalid confirmation token.");
            }
            
            // Parse timestamps
            if (!DateTime.TryParse(tokenTimestampString, out var tokenTimestamp))
            {
                throw new InvalidOperationException("Invalid token format.");
            }
            
            if (!DateTime.TryParse(expiryString, out var expiryTime))
            {
                throw new InvalidOperationException("Invalid token format.");
            }
            
            // Validate expiry
            if (DateTime.UtcNow > expiryTime)
            {
                throw new InvalidOperationException("Confirmation link has expired. Please request a new confirmation email.");
            }
            
            // Issue #4: Check if this is the latest token
            if (!_tokenTracker.IsLatestToken(request.Email, tokenTimestamp))
            {
                throw new InvalidOperationException("This confirmation link has been superseded by a newer one. Please use the latest confirmation email.");
            }
            
            // Confirm email with the standard token
            var result = await _userManager.ConfirmEmailAsync(user, standardToken);
            if (!result.Succeeded)
            {
                var msg = string.Join(";", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Confirm email failed: {msg}");
            }
            
            // Clear tracking after successful confirmation
            _tokenTracker.ClearToken(request.Email);
        }
        else if (tokenParts.Length == 3)
        {
            // Old format: userId|expiryTimestamp|standardToken (backward compatibility)
            var userId = tokenParts[0];
            var expiryString = tokenParts[1];
            var standardToken = tokenParts[2];
            
            // Validate user ID matches
            if (userId != user.Id.ToString())
            {
                throw new InvalidOperationException("Invalid confirmation token.");
            }
            
            // Validate expiry
            if (DateTime.TryParse(expiryString, out var expiryTime))
            {
                if (DateTime.UtcNow > expiryTime)
                {
                    throw new InvalidOperationException("Confirmation link has expired. Please request a new confirmation email.");
                }
            }
            
            // Use the standard token for confirmation
            var result = await _userManager.ConfirmEmailAsync(user, standardToken);
            if (!result.Succeeded)
            {
                var msg = string.Join(";", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Confirm email failed: {msg}");
            }
        }
        else
        {
            // Fallback to old token format for backward compatibility
            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                var msg = string.Join(";", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Confirm email failed: {msg}");
            }
        }
        
        return true;
    }
}
