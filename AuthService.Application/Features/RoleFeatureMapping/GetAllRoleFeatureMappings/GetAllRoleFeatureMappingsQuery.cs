using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RoleFeatureMapping.GetAllRoleFeatureMappings;

public sealed record GetAllRoleFeatureMappingsQuery : IRequest<List<RoleFeatureMappingDto>>;
