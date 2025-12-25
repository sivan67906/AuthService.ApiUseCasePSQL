namespace AuthService.Application.Features.Menu.GetUserRoles;

public record GetUserRolesQuery(Guid UserId) : IRequest<List<string>>;
