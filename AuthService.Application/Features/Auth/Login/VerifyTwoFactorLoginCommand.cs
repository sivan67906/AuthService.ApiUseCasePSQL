namespace AuthService.Application.Features.Auth.Login;

/// <summary>
/// Command to verify two-factor authentication code and complete login
/// </summary>
public sealed record VerifyTwoFactorLoginCommand(
    string Email,
    string TwoFactorToken,
    string Code,
    string TwoFactorType
) : IRequest<LoginResultDto>;