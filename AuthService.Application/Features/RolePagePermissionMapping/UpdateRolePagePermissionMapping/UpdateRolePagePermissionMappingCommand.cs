using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.UpdateRolePagePermissionMapping;

public sealed record UpdateRolePagePermissionMappingCommand(
    Guid Id,
    Guid RoleId,
    Guid PageId,
    Guid PermissionId,
    Guid? DepartmentId,
    bool IsActive
) : IRequest<RolePagePermissionMappingDto>;
