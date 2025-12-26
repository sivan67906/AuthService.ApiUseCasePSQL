namespace AuthService.Application.Features.Auth.Login;

/// <summary>
/// Command to verify two-factor authentication code and complete login
/// </summary>
public sealed record VerifyTwoFactorLoginCommand(
    string Email,
    string Code,
    string TwoFactorToken,
    string TwoFactorType
) : IRequest<LoginResultDto>;