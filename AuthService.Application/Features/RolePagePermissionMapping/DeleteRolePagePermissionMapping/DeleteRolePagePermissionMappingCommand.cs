namespace AuthService.Application.Features.RolePagePermissionMapping.DeleteRolePagePermissionMapping;

public sealed record DeleteRolePagePermissionMappingCommand(Guid Id) : IRequest<bool>;
