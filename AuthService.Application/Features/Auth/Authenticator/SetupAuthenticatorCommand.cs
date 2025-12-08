namespace AuthService.Application.Features.Auth.Authenticator;

/// <summary>
/// Command to generate authenticator setup information (secret key and QR code URI)
/// </summary>
public sealed record SetupAuthenticatorCommand(string UserId) : IRequest<AuthenticatorSetupDto>;
/// DTO containing authenticator setup information
public sealed record AuthenticatorSetupDto
{
    public required string SecretKey { get; init; }
    public required string QrCodeUri { get; init; }
    public required string ManualEntryKey { get; init; }
}
