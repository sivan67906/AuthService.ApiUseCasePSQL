using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth.TwoFactor;

public class VerifyTwoFactorCodeCommandHandler : IRequestHandler<VerifyTwoFactorCodeCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyTwoFactorCodeCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(VerifyTwoFactorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultEmailProvider,
            request.Code);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid 2FA code.");
        }

        return true;
    }
}

