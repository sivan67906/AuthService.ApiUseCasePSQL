namespace AuthService.Application.Features.Auth.Authenticator;

/// <summary>
/// Query to get the current authenticator status for a user
/// </summary>
public sealed record GetAuthenticatorStatusQuery(string UserId) : IRequest<AuthenticatorStatusDto>;
/// DTO containing authenticator status information
public sealed record AuthenticatorStatusDto
{
    public bool IsEnabled { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public string TwoFactorType { get; init; } = string.Empty;
}
