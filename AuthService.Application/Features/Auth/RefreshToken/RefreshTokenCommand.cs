namespace AuthService.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Auth.RefreshToken.RefreshTokenResultDto>;
