namespace AuthService.Application.Features.Auth.TwoFactor;

public record DisableTwoFactorCommand(string UserId, string? IpAddress = null) : IRequest<bool>;
