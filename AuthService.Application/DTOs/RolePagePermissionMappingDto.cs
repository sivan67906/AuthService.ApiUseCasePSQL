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

/// <summary>
/// Grouped representation of permissions for a Department-Role-Page combination
/// </summary>
public class RolePagePermissionGroupDto
{
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;
    public List<PermissionBadgeDto> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Permission badge information for display
/// </summary>
public class PermissionBadgeDto
{
    public Guid Id { get; set; }
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionCode { get; set; } = string.Empty;
    public string BadgeColor { get; set; } = "primary";
}

/// <summary>
/// Request for batch create/update of permissions for a Department-Role-Page combination
/// </summary>
public class CreateOrUpdatePermissionBatchDto
{
    public Guid? DepartmentId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PageId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}
