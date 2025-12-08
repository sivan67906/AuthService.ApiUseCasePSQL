using AuthService.Application.Features.Role.CreateRole;

namespace AuthService.Application.Features.Role.GetAllRoles;
public sealed record GetAllRolesQuery : IRequest<List<RoleDto>>;
