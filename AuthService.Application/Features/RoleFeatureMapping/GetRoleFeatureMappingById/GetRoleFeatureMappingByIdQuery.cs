using AuthService.Application.DTOs;

namespace AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingById;

public sealed record GetRoleFeatureMappingByIdQuery(Guid Id) : IRequest<RoleFeatureMappingDto>;
