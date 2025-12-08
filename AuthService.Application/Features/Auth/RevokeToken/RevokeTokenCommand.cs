namespace AuthService.Application.Features.Auth.RevokeToken;

public record RevokeTokenCommand(string RefreshToken) : IRequest<bool>;
