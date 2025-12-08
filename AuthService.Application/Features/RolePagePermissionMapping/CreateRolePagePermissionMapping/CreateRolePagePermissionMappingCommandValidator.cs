namespace AuthService.Application.Features.RolePagePermissionMapping.CreateRolePagePermissionMapping;

public sealed class CreateRolePagePermissionMappingCommandValidator : AbstractValidator<CreateRolePagePermissionMappingCommand>
{
    public CreateRolePagePermissionMappingCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("PermissionId is required");
    }
}
