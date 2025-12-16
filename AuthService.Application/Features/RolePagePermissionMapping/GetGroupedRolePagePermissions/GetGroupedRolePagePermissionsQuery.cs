using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetGroupedRolePagePermissions;

public record GetGroupedRolePagePermissionsQuery() : IRequest<List<RolePagePermissionGroupDto>>;
