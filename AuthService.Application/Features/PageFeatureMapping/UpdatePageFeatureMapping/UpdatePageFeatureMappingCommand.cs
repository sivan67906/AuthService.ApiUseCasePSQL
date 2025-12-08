using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

namespace AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;

/// <summary>
/// Command to update a page-feature mapping
/// </summary>
public sealed record UpdatePageFeatureMappingCommand(
    Guid Id,
    Guid PageId,
    Guid FeatureId
) : IRequest<PageFeatureMappingDto>;