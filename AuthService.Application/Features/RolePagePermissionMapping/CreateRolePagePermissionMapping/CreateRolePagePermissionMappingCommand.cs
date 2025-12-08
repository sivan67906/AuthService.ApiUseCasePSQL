using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.CreateRolePagePermissionMapping;

public sealed record CreateRolePagePermissionMappingCommand(
    Guid RoleId,
    Guid PageId,
    Guid PermissionId,
    Guid? DepartmentId,
    bool IsActive = true
) : IRequest<RolePagePermissionMappingDto>;
