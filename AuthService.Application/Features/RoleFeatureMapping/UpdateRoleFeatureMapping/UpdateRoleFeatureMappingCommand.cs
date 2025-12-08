using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RoleFeatureMapping.UpdateRoleFeatureMapping;

public sealed record UpdateRoleFeatureMappingCommand(
    Guid Id,
    Guid RoleId,
    Guid FeatureId,
    Guid? DepartmentId,
    bool IsActive
) : IRequest<RoleFeatureMappingDto>;
