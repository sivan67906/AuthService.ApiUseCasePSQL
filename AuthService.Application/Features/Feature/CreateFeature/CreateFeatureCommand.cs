namespace AuthService.Application.Features.Feature.CreateFeature;

/// <summary>
/// Command to create a new feature
/// </summary>
public sealed record CreateFeatureCommand(
    string Name,
    string? Description,
    string? RouteUrl,
    bool IsMainMenu,
    Guid? ParentFeatureId,
    int DisplayOrder,
    int Level,
    string? Icon,
    bool IsActive = true
) : IRequest<FeatureDto>;