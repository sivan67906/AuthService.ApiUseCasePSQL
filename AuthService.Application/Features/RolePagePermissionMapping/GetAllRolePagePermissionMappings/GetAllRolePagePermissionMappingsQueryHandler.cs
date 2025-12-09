using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetAllRolePagePermissionMappings;
public sealed class GetAllRolePagePermissionMappingsQueryHandler : IRequestHandler<GetAllRolePagePermissionMappingsQuery, List<RolePagePermissionMappingDto>>
{
    private readonly IAppDbContext _db;
    public GetAllRolePagePermissionMappingsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<RolePagePermissionMappingDto>> Handle(GetAllRolePagePermissionMappingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.RolePagePermissionMappings.AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Page)
            .Include(x => x.Permission)
            .Include(x => x.Department)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Adapt<List<RolePagePermissionMappingDto>>();
    }
}
