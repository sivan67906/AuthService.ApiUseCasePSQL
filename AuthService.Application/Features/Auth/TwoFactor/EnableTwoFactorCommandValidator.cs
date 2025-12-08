namespace AuthService.Application.Features.Auth.TwoFactor;

public class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
