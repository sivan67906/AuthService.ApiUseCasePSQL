namespace AuthService.Domain.Entities;

public sealed class Page : BaseEntity
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? MenuContext { get; set; }  // Tracks which menu/submenu this page belongs to
    public string? ApiEndpoint { get; set; }  // Actual API endpoint for this page
    public string? HttpMethod { get; set; }   // GET, POST, PUT, DELETE
    
    // Navigation properties
    public ICollection<PagePermissionMapping> PagePermissions { get; init; } = [];
    public ICollection<PageFeatureMapping> PageFeatures { get; init; } = [];
    public ICollection<RolePagePermissionMapping> RolePagePermissionMappings { get; init; } = [];
}
