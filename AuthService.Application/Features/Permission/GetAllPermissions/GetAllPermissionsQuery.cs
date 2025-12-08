using AuthService.Application.Features.Permission.CreatePermission;

namespace AuthService.Application.Features.Permission.GetAllPermissions;
public sealed record GetAllPermissionsQuery : IRequest<List<PermissionDto>>;
