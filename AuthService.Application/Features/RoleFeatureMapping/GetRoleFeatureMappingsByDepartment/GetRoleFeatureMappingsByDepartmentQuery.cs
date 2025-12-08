using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByDepartment;

public sealed record GetRoleFeatureMappingsByDepartmentQuery(Guid DepartmentId) : IRequest<List<RoleFeatureMappingDto>>;
