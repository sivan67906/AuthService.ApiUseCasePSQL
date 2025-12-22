using System.Text.RegularExpressions;

namespace AuthService.Application.Features.Department.UpdateDepartment;

public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    private static readonly Regex EmojiPattern = new(@"[\uD83C-\uDBFF][\uDC00-\uDFFF]", RegexOptions.Compiled);

    public UpdateDepartmentCommandValidator()
    {
        // Code is immutable after creation - not included in update

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .Must(name => !EmojiPattern.IsMatch(name ?? "")).WithMessage("Name cannot contain emojis");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .Must(desc => string.IsNullOrEmpty(desc) || !EmojiPattern.IsMatch(desc)).WithMessage("Description cannot contain emojis");
    }
}
