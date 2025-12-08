namespace AuthService.Application.Features.Role.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest<bool>;
