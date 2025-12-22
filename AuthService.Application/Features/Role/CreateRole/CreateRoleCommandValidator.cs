using System.Text.RegularExpressions;

namespace AuthService.Application.Features.Role.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private static readonly Regex CodePattern = new(@"^[A-Z0-9\-_]+$", RegexOptions.Compiled);
    private static readonly Regex EmojiPattern = new(@"[\uD83C-\uDBFF][\uDC00-\uDFFF]", RegexOptions.Compiled);

    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(10).WithMessage("Code must not exceed 10 characters")
            .Must(code => CodePattern.IsMatch(code ?? "")).WithMessage("Code must contain only uppercase letters, numbers, hyphens, and underscores")
            .Must(code => !EmojiPattern.IsMatch(code ?? "")).WithMessage("Code cannot contain emojis");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(256).WithMessage("Role name must not exceed 256 characters")
            .Must(name => !EmojiPattern.IsMatch(name ?? "")).WithMessage("Name cannot contain emojis");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .Must(desc => string.IsNullOrEmpty(desc) || !EmojiPattern.IsMatch(desc)).WithMessage("Description cannot contain emojis")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
