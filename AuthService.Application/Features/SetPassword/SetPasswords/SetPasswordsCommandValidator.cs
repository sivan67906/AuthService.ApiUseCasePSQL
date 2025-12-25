namespace AuthService.Application.Features.SetPassword.SetPasswords;

public class SetPasswordsCommandValidator : AbstractValidator<SetPasswordsCommand>
{
    public SetPasswordsCommandValidator()
    {
        RuleFor(x => x.Emails)
            .NotNull()
            .NotEmpty()
            .WithMessage("Email list cannot be empty");

        RuleForEach(x => x.Emails)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email address format");
    }
}
