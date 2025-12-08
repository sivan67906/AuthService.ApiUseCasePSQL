namespace AuthService.Application.Features.Auth.TwoFactor;

public class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
