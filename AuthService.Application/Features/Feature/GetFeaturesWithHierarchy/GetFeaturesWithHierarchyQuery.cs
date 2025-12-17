namespace AuthService.Application.Features.Feature.GetFeaturesWithHierarchy;

/// <summary>
/// Enhanced FeatureDto with hierarchical display
/// Main Menu: "Finance Management (Main Menu)"
/// SubMenu: "Finance Management → Test Categories"
/// </summary>
public sealed record FeatureWithHierarchyDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsMainMenu,
    Guid? ParentFeatureId,
    string? ParentFeatureName,
    int DisplayOrder,
    string? Icon,
    bool IsActive,
    string? RouteUrl,
    int Level,
    string DisplayName  // Enhanced display name with hierarchy
);

public sealed record GetFeaturesWithHierarchyQuery : IRequest<List<FeatureWithHierarchyDto>>;
