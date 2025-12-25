namespace AuthService.Application.Features.Menu.CheckPageAccess;

public record CheckPageAccessQuery(Guid UserId, string PageName) : IRequest<bool>;
