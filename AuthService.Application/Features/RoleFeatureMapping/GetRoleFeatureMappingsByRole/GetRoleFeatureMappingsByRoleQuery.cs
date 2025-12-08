using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByRole;

public sealed record GetRoleFeatureMappingsByRoleQuery(Guid RoleId) : IRequest<List<RoleFeatureMappingDto>>;
