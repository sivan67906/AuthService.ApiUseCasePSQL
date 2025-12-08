namespace AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;

public sealed class UpdatePageFeatureMappingCommandValidator : AbstractValidator<UpdatePageFeatureMappingCommand>
{
    public UpdatePageFeatureMappingCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required");
        RuleFor(x => x.FeatureId)
            .NotEmpty().WithMessage("FeatureId is required");
    }
}
