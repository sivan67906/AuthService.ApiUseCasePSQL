namespace AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;

public sealed record GetUsersWithoutRolesQuery : IRequest<List<UserWithoutRoleDto>>;
