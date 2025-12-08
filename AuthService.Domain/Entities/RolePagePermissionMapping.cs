namespace AuthService.Domain.Entities;

/// <summary>
/// Maps specific Permissions to Pages for each Role
/// This is the core entity that defines what a Role can do on each Page
/// </summary>
public sealed class RolePagePermissionMapping : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PageId { get; set; }
    public Guid PermissionId { get; set; }
    public Guid? DepartmentId { get; set; }  // Null for SuperAdmin, required for DepartmentAdmin roles
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ApplicationRole Role { get; set; } = null!;
    public Page Page { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    public Department? Department { get; set; }
}
