namespace AuthService.Application.Features.Feature.DeleteFeature;

public sealed record DeleteFeatureCommand(Guid Id) : IRequest<bool>;
