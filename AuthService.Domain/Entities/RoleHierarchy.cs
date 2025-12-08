namespace AuthService.Domain.Entities;

/// <summary>
/// Represents the hierarchical relationship between roles within a department
/// Each department has its own role hierarchy
/// </summary>
public sealed class RoleHierarchy : BaseEntity
{
    public Guid DepartmentId { get; set; }  // Each hierarchy belongs to a department
    public Guid ParentRoleId { get; set; }   // Parent role in hierarchy
    public Guid ChildRoleId { get; set; }    // Child role in hierarchy
    public int Level { get; set; }           // Hierarchy level (0 = top, 1 = next level, etc.)
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Department Department { get; set; } = null!;
    public ApplicationRole ParentRole { get; set; } = null!;
    public ApplicationRole ChildRole { get; set; } = null!;
}
