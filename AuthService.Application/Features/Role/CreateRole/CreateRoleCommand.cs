namespace AuthService.Application.Features.Role.CreateRole;

/// <summary>
/// Command to create a new role
/// </summary>
public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    Guid? DepartmentId
) : IRequest<RoleDto>;