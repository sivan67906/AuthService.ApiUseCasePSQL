using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>, IAuditableEntity, ISoftDeletable
{
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<RolePermissionMapping> RolePermissions { get; init; } = [];
    public ICollection<UserRoleMapping> UserRoleMappings { get; init; } = [];
    public ICollection<RoleDepartmentMapping> RoleDepartmentMappings { get; init; } = [];
    public ICollection<RoleHierarchy> ParentRoleHierarchies { get; init; } = [];
    public ICollection<RoleHierarchy> ChildRoleHierarchies { get; init; } = [];
    public ICollection<RoleFeatureMapping> RoleFeatureMappings { get; init; } = [];
    public ICollection<RolePagePermissionMapping> RolePagePermissionMappings { get; init; } = [];
}
