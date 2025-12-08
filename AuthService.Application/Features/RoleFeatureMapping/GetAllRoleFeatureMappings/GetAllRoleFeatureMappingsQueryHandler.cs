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
        var mappings = await _db.RoleFeatureMappings.AsNoTracking()
            .Include(rfm => rfm.Role)
            .Include(rfm => rfm.Feature)
            .Include(rfm => rfm.Department)
            .Where(x => !x.IsDeleted)
            .Select(rfm => new RoleFeatureMappingDto
            {
                Id = rfm.Id,
                RoleId = rfm.RoleId,
                RoleName = rfm.Role.Name ?? string.Empty,
                FeatureId = rfm.FeatureId,
                FeatureName = rfm.Feature.Name,
                DepartmentId = rfm.DepartmentId,
                DepartmentName = rfm.Department != null ? rfm.Department.Name : null,
                IsActive = rfm.IsActive,
                CreatedAt = rfm.CreatedAt,
                UpdatedAt = rfm.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return mappings;
    }
}
