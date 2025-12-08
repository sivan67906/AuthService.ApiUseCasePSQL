namespace AuthService.Domain.Entities;

public sealed class Department : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<ApplicationRole> Roles { get; init; } = [];
    public ICollection<UserRoleMapping> UserRoleMappings { get; init; } = [];
    public ICollection<RoleDepartmentMapping> RoleDepartmentMappings { get; init; } = [];
    public ICollection<RoleHierarchy> RoleHierarchies { get; init; } = [];
    public ICollection<RoleFeatureMapping> RoleFeatureMappings { get; init; } = [];
    public ICollection<RolePagePermissionMapping> RolePagePermissionMappings { get; init; } = [];
}
