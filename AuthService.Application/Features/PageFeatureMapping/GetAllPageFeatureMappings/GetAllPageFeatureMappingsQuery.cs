using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

namespace AuthService.Application.Features.PageFeatureMapping.GetAllPageFeatureMappings;
public sealed record GetAllPageFeatureMappingsQuery : IRequest<List<PageFeatureMappingDto>>;
