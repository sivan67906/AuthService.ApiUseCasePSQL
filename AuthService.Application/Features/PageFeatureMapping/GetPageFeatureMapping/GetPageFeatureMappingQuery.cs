using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

namespace AuthService.Application.Features.PageFeatureMapping.GetPageFeatureMapping;
public sealed record GetPageFeatureMappingQuery(Guid Id) : IRequest<PageFeatureMappingDto?>;
