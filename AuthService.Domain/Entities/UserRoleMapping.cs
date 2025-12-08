namespace AuthService.Domain.Entities;

public sealed class UserRoleMapping : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? DepartmentId { get; set; }  // Changed to nullable - SuperAdmin has no department
    public string AssignedByEmail { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
    public Department? Department { get; set; }  // Also made nullable
}
