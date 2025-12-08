namespace AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;

/// <summary>
/// Query to get users without roles from Command DB for immediate consistency after role assignment
/// </summary>
public sealed record GetUsersWithoutRolesFromCommandDbQuery : IRequest<List<UserWithoutRoleDto>>;
