namespace AuthService.Application.Features.Feature.CreateFeature;

/// <summary>
/// Data transfer object for a feature
/// </summary>
public sealed record FeatureDto(
    Guid Id,
    string Name,
    string? Description,
    string? RouteUrl,
    bool IsMainMenu,
    Guid? ParentFeatureId,
    string? ParentFeatureName,
    int DisplayOrder,
    int Level,
    string? Icon,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);