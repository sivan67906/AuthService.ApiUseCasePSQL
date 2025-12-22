namespace AuthService.Application.Features.Permission.CreatePermission;

/// <summary>
/// Command to create a new permission
/// </summary>
public sealed record CreatePermissionCommand(
    string Code,
    string Name,
    string? Description
) : IRequest<PermissionDto>;