namespace AuthService.Application.Features.Auth.TwoFactor;

public record GenerateTwoFactorCodeCommand(string UserId) : IRequest<bool>;
