namespace AuthService.Application.Features.PageFeatureMapping.DeletePageFeatureMapping;

public sealed record DeletePageFeatureMappingCommand(Guid Id) : IRequest<bool>;
