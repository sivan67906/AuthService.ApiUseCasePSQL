namespace AuthService.Application.Features.Auth.Login;

public sealed record ResendTwoFactorLoginCodeCommand(
    string Email,
    string TwoFactorToken
) : IRequest<ResendTwoFactorCodeResultDto>;

/// <summary>
/// Result DTO for resending 2FA code - returns new token so old ones are invalidated
/// </summary>
public sealed record ResendTwoFactorCodeResultDto(
    string NewTwoFactorToken,
    string Message
);