using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Permission.CreatePermission;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.GetAllPermissions;
public sealed class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    private readonly IAppDbContext _db;
    public GetAllPermissionsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.Permissions.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Adapt<List<PermissionDto>>();
}
}
