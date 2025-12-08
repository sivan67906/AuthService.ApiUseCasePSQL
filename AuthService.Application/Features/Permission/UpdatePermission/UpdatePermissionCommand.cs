using AuthService.Application.Features.Permission.CreatePermission;

namespace AuthService.Application.Features.Permission.UpdatePermission;

/// <summary>
/// Command to update an existing permission
/// </summary>
public sealed record UpdatePermissionCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<PermissionDto>;