using AuthService.Application.Features.Feature.CreateFeature;

namespace AuthService.Application.Features.Feature.UpdateFeature;

/// <summary>
/// Command to update an existing feature
/// </summary>
public sealed record UpdateFeatureCommand(
    Guid Id,
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