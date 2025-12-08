namespace AuthService.Domain.Entities;

/// <summary>
/// Maps which Features (Menu/SubMenu) are accessible by each Role within a Department
/// </summary>
public sealed class RoleFeatureMapping : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid FeatureId { get; set; }
    public Guid? DepartmentId { get; set; }  // Null for SuperAdmin, required for DepartmentAdmin
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ApplicationRole Role { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
    public Department? Department { get; set; }
}
