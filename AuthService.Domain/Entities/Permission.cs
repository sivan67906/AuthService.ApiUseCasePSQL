namespace AuthService.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    // Navigation properties
    public ICollection<RolePermissionMapping> RolePermissions { get; init; } = [];
    public ICollection<PagePermissionMapping> PagePermissions { get; init; } = [];
}
