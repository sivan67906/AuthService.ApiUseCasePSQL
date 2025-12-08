namespace AuthService.Application.DTOs;

public class RolePagePermissionMappingDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRolePagePermissionMappingDto
{
    public Guid RoleId { get; set; }
    public Guid PageId { get; set; }
    public Guid PermissionId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateRolePagePermissionMappingDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid PageId { get; set; }
    public Guid PermissionId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; }
}
