namespace AuthService.Domain.Entities;

public sealed class Feature : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsMainMenu { get; set; }
    public Guid? ParentFeatureId { get; set; }
    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RouteUrl { get; set; }  // For navigation to specific page
    public int Level { get; set; }  // 0 for main menu, 1+ for submenus
    
    // Navigation properties
    public Feature? ParentFeature { get; set; }
    public ICollection<Feature> SubFeatures { get; init; } = [];
    public ICollection<PageFeatureMapping> PageFeatures { get; init; } = [];
    public ICollection<RoleFeatureMapping> RoleFeatureMappings { get; init; } = [];
}
