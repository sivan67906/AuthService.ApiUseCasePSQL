namespace AuthService.Application.Features.Auth.Login;

public sealed record ResendTwoFactorLoginCodeCommand(
    string Email,
    string TwoFactorToken
) : IRequest<Unit>;