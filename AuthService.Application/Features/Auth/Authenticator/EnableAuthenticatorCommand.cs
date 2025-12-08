namespace AuthService.Application.Features.Auth.Authenticator;

/// <summary>
/// Command to enable authenticator app 2FA after verifying the code
/// </summary>
public sealed record EnableAuthenticatorCommand(string UserId, string VerificationCode, string? IpAddress = null) : IRequest<bool>;
