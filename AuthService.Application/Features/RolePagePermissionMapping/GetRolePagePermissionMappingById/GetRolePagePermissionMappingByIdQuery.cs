using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingById;

public sealed record GetRolePagePermissionMappingByIdQuery(Guid Id) : IRequest<RolePagePermissionMappingDto>;
