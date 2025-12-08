namespace AuthService.Application.Features.Auth.EmailConfirmation;

public record SendEmailConfirmationCommand(string Email, string CallbackBaseUrl) : IRequest<bool>;
