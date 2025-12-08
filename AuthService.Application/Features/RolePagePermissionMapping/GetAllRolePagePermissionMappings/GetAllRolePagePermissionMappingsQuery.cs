using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetAllRolePagePermissionMappings;

public sealed record GetAllRolePagePermissionMappingsQuery : IRequest<List<RolePagePermissionMappingDto>>;
