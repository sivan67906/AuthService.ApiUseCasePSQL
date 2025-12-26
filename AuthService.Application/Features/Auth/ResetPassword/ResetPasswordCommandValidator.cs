namespace AuthService.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword).NotEmpty().MinimumLength(6)
        .Equal(x => x.NewPassword)
        .WithMessage("Password and confirmation password do not match.");
    }
}
