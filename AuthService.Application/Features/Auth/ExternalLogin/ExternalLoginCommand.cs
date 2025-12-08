namespace AuthService.Application.Features.Auth.ExternalLogin;

public record ExternalLoginCommand(string Provider, string ProviderUserId, string Email) : IRequest<Auth.Login.LoginResultDto>;
