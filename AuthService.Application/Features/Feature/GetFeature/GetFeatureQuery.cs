using AuthService.Application.Features.Feature.CreateFeature;

namespace AuthService.Application.Features.Feature.GetFeature;
public sealed record GetFeatureQuery(Guid Id) : IRequest<FeatureDto?>;
