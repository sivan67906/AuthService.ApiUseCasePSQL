using AuthService.Application.Features.Role.CreateRole;

namespace AuthService.Application.Features.Role.UpdateRole;

/// <summary>
/// Command to update an existing role
/// </summary>
public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    Guid? DepartmentId
) : IRequest<RoleDto>;