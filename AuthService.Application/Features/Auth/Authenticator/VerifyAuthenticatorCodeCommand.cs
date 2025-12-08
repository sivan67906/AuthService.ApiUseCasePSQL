namespace AuthService.Application.Features.Auth.Authenticator;

/// <summary>
/// Command to verify authenticator code during login
/// </summary>
public sealed record VerifyAuthenticatorCodeCommand(string UserId, string Code) : IRequest<bool>;
