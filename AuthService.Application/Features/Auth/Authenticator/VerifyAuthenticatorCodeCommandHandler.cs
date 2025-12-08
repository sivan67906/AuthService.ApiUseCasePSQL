using Microsoft.AspNetCore.Identity;
using OtpNet;

namespace AuthService.Application.Features.Auth.Authenticator;

public sealed class VerifyAuthenticatorCodeCommandHandler : IRequestHandler<VerifyAuthenticatorCodeCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyAuthenticatorCodeCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(VerifyAuthenticatorCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.AuthenticatorEnabled || string.IsNullOrEmpty(user.AuthenticatorSecretKey))
        {
            throw new InvalidOperationException("Authenticator app is not enabled for this user.");
        }

        // Verify the TOTP code
        var secretBytes = Base32Encoding.ToBytes(user.AuthenticatorSecretKey);
        var totp = new Totp(secretBytes);

        // Allow a window of +/- 1 time step for network delays
        var isValid = totp.VerifyTotp(request.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid authentication code.");
        }

        return true;
    }
}

