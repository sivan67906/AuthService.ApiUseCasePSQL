namespace AuthService.Application.Features.RoleHierarchyMapping;

public class RoleHierarchyDto
{
    public Guid Id { get; set; }
    public Guid ParentRoleId { get; set; }
    public string ParentRoleName { get; set; } = string.Empty;
    public Guid ChildRoleId { get; set; }
    public string ChildRoleName { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
