namespace AuthService.Application.Features.Permission.DeletePermission;

public sealed record DeletePermissionCommand(Guid Id) : IRequest<bool>;
