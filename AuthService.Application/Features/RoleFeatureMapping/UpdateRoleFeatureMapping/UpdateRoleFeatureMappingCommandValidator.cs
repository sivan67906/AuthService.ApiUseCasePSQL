namespace AuthService.Application.Features.RoleFeatureMapping.UpdateRoleFeatureMapping;

public sealed class UpdateRoleFeatureMappingCommandValidator : AbstractValidator<UpdateRoleFeatureMappingCommand>
{
    public UpdateRoleFeatureMappingCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.FeatureId)
            .NotEmpty().WithMessage("FeatureId is required");
    }
}
