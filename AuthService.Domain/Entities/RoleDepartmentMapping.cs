namespace AuthService.Domain.Entities;

public sealed class RoleDepartmentMapping : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid DepartmentId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    // Navigation properties
    public ApplicationRole Role { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
