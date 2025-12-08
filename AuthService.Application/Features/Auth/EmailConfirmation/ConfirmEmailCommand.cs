namespace AuthService.Application.Features.Auth.EmailConfirmation;

public record ConfirmEmailCommand(string Email, string Token) : IRequest<bool>;
