using AuthService.Application.Features.Role.CreateRole;

namespace AuthService.Application.Features.Role.GetRoleById;
public sealed record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleDto>;
