using Microsoft.AspNetCore.Identity;
using OtpNet;

namespace AuthService.Application.Features.Auth.Authenticator;

public sealed class SetupAuthenticatorCommandHandler : IRequestHandler<SetupAuthenticatorCommand, AuthenticatorSetupDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private const string Issuer = "AuthManagement";

    public SetupAuthenticatorCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthenticatorSetupDto> Handle(SetupAuthenticatorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found.");

        // Generate a new secret key if one doesn't exist or regenerate on setup
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        // Store the secret temporarily (will be confirmed when user verifies)
        user.AuthenticatorSecretKey = base32Secret;
        await _userManager.UpdateAsync(user);

        // Generate the otpauth URI for QR code
        var email = user.Email ?? user.UserName ?? "user";
        var qrCodeUri = GenerateQrCodeUri(email, base32Secret);

        // Format manual entry key with spaces for readability
        var manualEntryKey = FormatManualEntryKey(base32Secret);

        return new AuthenticatorSetupDto
        {
            SecretKey = base32Secret,
            QrCodeUri = qrCodeUri,
            ManualEntryKey = manualEntryKey
        };
    }

    private static string GenerateQrCodeUri(string email, string secret)
    {
        // Format: otpauth://totp/ISSUER:ACCOUNT?secret=SECRET&issuer=ISSUER&algorithm=SHA1&digits=6&period=30
        var encodedIssuer = Uri.EscapeDataString(Issuer);
        var encodedEmail = Uri.EscapeDataString(email);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    private static string FormatManualEntryKey(string secret)
    {
        // Add spaces every 4 characters for readability
        return string.Join(" ", Enumerable.Range(0, (secret.Length + 3) / 4)
            .Select(i => secret.Substring(i * 4, Math.Min(4, secret.Length - i * 4))));
    }
}