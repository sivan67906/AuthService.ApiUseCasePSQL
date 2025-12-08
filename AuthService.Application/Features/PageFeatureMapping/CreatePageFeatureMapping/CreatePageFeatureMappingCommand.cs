namespace AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

public sealed record CreatePageFeatureMappingCommand(
    Guid PageId,
    Guid FeatureId
) : IRequest<PageFeatureMappingDto>;