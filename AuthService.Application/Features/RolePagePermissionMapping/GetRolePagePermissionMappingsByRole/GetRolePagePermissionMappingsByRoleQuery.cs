using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRole;

public sealed record GetRolePagePermissionMappingsByRoleQuery(Guid RoleId) : IRequest<List<RolePagePermissionMappingDto>>;
