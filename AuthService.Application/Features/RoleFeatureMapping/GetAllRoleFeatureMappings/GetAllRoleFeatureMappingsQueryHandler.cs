using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.GetAllRoleFeatureMappings;
public sealed class GetAllRoleFeatureMappingsQueryHandler : IRequestHandler<GetAllRoleFeatureMappingsQuery, List<RoleFeatureMappingDto>>
{
    private readonly IAppDbContext _db;
    public GetAllRoleFeatureMappingsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<RoleFeatureMappingDto>> Handle(GetAllRoleFeatureMappingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.RoleFeatureMappings.AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Feature)
            .Include(x => x.Department)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Adapt<List<RoleFeatureMappingDto>>();
    }
}
