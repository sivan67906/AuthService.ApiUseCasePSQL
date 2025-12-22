using FluentValidation;
using System.Text.RegularExpressions;

namespace AuthService.Application.Features.Role.UpdateRole;
public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private static readonly Regex EmojiPattern = new(@"[\uD83C-\uDBFF][\uDC00-\uDFFF]", RegexOptions.Compiled);

    public UpdateRoleCommandValidator()
    {
        // Code is immutable after creation - not included in update

        RuleFor(x => x.RoleId).NotEmpty();
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256)
            .Must(name => !EmojiPattern.IsMatch(name ?? "")).WithMessage("Name cannot contain emojis");
        
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .Must(desc => string.IsNullOrEmpty(desc) || !EmojiPattern.IsMatch(desc)).WithMessage("Description cannot contain emojis");
    }
}
