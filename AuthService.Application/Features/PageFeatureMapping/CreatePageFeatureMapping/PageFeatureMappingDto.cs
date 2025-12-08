namespace AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;

/// <summary>
/// Data transfer object for page-feature mapping
/// </summary>
public sealed record PageFeatureMappingDto(
    Guid Id,
    Guid PageId,
    Guid FeatureId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);