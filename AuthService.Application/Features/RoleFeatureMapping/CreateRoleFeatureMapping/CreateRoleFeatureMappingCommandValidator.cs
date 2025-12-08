namespace AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;

public sealed class CreateRoleFeatureMappingCommandValidator : AbstractValidator<CreateRoleFeatureMappingCommand>
{
    public CreateRoleFeatureMappingCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.FeatureId)
            .NotEmpty().WithMessage("FeatureId is required");
    }
}
