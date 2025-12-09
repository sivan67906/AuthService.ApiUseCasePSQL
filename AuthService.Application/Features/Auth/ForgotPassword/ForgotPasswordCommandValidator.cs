namespace AuthService.Application.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.CallbackBaseUrl)
            .NotEmpty().WithMessage("Callback URL is required.")
            .Must(BeValidUri).WithMessage("Callback URL must be a valid absolute URL.");

        // IpAddress is optional
    }
    private bool BeValidUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
