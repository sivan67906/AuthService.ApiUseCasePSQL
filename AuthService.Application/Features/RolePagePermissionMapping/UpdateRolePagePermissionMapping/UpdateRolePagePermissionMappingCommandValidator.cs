namespace AuthService.Application.Features.RolePagePermissionMapping.UpdateRolePagePermissionMapping;

public sealed class UpdateRolePagePermissionMappingCommandValidator : AbstractValidator<UpdateRolePagePermissionMappingCommand>
{
    public UpdateRolePagePermissionMappingCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("PermissionId is required");
    }
}
