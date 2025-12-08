namespace AuthService.Application.Features.Role.CreateRole;

/// <summary>
/// Data transfer object for a role
/// </summary>
public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? DepartmentId,
    string? DepartmentName
);