using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class VerifyTwoFactorCodeCommandHandler : IRequestHandler<VerifyTwoFactorCodeCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITwoFactorCodeThrottlingService _twoFactorThrottlingService;

    public VerifyTwoFactorCodeCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITwoFactorCodeThrottlingService twoFactorThrottlingService)
    {
        _userManager = userManager;
        _twoFactorThrottlingService = twoFactorThrottlingService;
    }

    public async Task<bool> Handle(VerifyTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        // Verify email-based 2FA code with custom validation
        // This ensures only the latest code works and validates expiry (1 hour)
        var isValid = _twoFactorThrottlingService.ValidateCode(user.Email!, request.Code);

        // If our custom validation passed, also verify with Identity for additional security
        if (isValid)
        {
            isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                request.Code);
        }

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid 2FA code.");
        }

        // Clear throttling attempts after successful verification
        _twoFactorThrottlingService.ClearAttempts(user.Email!);

        return true;
    }
}

