namespace AuthService.Application.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email, string CallbackBaseUrl, string? IpAddress) : IRequest<bool>;
