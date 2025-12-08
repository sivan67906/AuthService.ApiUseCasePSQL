namespace AuthService.Application.Features.Auth.TwoFactor;

public record VerifyTwoFactorCodeCommand(string UserId, string Code) : IRequest<bool>;
