using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByDepartment;

public sealed record GetRolePagePermissionMappingsByDepartmentQuery(Guid DepartmentId) : IRequest<List<RolePagePermissionMappingDto>>;
