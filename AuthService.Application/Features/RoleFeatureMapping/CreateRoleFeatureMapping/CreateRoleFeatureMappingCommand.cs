using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;

public sealed record CreateRoleFeatureMappingCommand(
    Guid RoleId,
    Guid FeatureId,
    Guid? DepartmentId,
    bool IsActive = true
) : IRequest<RoleFeatureMappingDto>;
