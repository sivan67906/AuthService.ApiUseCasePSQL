namespace AuthService.Application.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CallbackBaseUrl)
            .NotEmpty()
            .Must(BeValidUri).WithMessage("Callback URL must be a valid absolute URL.");
        // IpAddress is optional
    }
    private bool BeValidUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
