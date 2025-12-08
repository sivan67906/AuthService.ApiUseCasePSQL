namespace AuthService.Application.Features.Auth.TwoFactor;

public record EnableTwoFactorCommand(string UserId, string? IpAddress = null) : IRequest<bool>;
