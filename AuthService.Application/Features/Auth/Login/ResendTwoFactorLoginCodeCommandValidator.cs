using FluentValidation;

namespace AuthService.Application.Features.Auth.Login;
public sealed class ResendTwoFactorLoginCodeCommandValidator : AbstractValidator<ResendTwoFactorLoginCodeCommand>
{
    public ResendTwoFactorLoginCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.TwoFactorToken)
            .NotEmpty().WithMessage("Two-factor token is required.");
    }
}
