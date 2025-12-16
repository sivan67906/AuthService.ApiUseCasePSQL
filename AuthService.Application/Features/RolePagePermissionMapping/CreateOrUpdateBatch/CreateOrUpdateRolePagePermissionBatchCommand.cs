using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RolePagePermissionMapping.CreateOrUpdateBatch;

public record CreateOrUpdateRolePagePermissionBatchCommand(
    Guid? DepartmentId,
    Guid RoleId,
    Guid PageId,
    List<Guid> PermissionIds
) : IRequest<List<RolePagePermissionMappingDto>>;
