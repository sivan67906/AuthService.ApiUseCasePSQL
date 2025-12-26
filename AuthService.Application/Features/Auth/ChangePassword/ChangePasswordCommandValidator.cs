namespace AuthService.Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current Password is required.").MinimumLength(6);
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New Password is required.").MinimumLength(6);
    }
}
