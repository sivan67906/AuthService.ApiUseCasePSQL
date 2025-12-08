using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByRoleAndPage;

public sealed record GetRolePagePermissionMappingsByRoleAndPageQuery(Guid RoleId, Guid PageId) : IRequest<List<RolePagePermissionMappingDto>>;
