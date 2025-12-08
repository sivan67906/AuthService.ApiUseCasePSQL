using AuthService.Application.Features.Feature.CreateFeature;

namespace AuthService.Application.Features.Feature.GetAllFeatures;
public sealed record GetAllFeaturesQuery : IRequest<List<FeatureDto>>;
