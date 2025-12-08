namespace AuthService.Application.Features.Auth.Authenticator;

/// <summary>
/// Command to disable authenticator app 2FA
/// </summary>
public sealed record DisableAuthenticatorCommand(string UserId, string? IpAddress = null) : IRequest<bool>;
