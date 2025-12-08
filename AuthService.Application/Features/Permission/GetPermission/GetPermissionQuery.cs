using AuthService.Application.Features.Permission.CreatePermission;

namespace AuthService.Application.Features.Permission.GetPermission;
public sealed record GetPermissionQuery(Guid Id) : IRequest<PermissionDto?>;
