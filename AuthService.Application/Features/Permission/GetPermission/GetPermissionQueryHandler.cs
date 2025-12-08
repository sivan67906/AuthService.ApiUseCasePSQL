using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Permission.CreatePermission;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.GetPermission;
public sealed class GetPermissionQueryHandler : IRequestHandler<GetPermissionQuery, PermissionDto?>
{
    private readonly IAppDbContext _db;
    public GetPermissionQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PermissionDto?> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Permissions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        return entity?.Adapt<PermissionDto>();
}
}
