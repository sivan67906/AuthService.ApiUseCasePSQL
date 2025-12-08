namespace AuthService.Application.Features.RoleFeatureMapping.DeleteRoleFeatureMapping;

public sealed record DeleteRoleFeatureMappingCommand(Guid Id) : IRequest<bool>;
