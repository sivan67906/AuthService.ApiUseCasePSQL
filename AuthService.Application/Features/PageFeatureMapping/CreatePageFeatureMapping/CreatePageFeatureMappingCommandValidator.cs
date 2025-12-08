namespace AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

public sealed class CreatePageFeatureMappingCommandValidator : AbstractValidator<CreatePageFeatureMappingCommand>
{
    public CreatePageFeatureMappingCommandValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required");
        RuleFor(x => x.FeatureId)
            .NotEmpty().WithMessage("FeatureId is required");
    }
}
