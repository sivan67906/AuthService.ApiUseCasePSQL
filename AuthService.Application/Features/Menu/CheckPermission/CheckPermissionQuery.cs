namespace AuthService.Application.Features.Menu.CheckPermission;

public record CheckPermissionQuery(Guid UserId, string PermissionName) : IRequest<bool>;
