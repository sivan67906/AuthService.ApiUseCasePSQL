namespace AuthService.Application.Features.Menu.GetPagePermissions;

public record GetPagePermissionsQuery(Guid UserId, string PageName) : IRequest<PagePermissionsDto>;
