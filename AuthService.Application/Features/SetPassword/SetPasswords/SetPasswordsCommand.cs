namespace AuthService.Application.Features.SetPassword.SetPasswords;

public record SetPasswordsCommand(List<string> Emails) : IRequest<string>;
