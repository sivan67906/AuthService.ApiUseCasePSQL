using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByRole;

public sealed class GetRoleFeatureMappingsByRoleQueryHandler : IRequestHandler<GetRoleFeatureMappingsByRoleQuery, List<RoleFeatureMappingDto>>
{
    private readonly IAppDbContext _db;

    public GetRoleFeatureMappingsByRoleQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoleFeatureMappingDto>> Handle(GetRoleFeatureMappingsByRoleQuery request, CancellationToken cancellationToken)
    {
        var mappings = await _db.RoleFeatureMappings.AsNoTracking()
            .Include(rfm => rfm.Role)
            .Include(rfm => rfm.Feature)
            .Include(rfm => rfm.Department)
            .Where(rfm => rfm.RoleId == request.RoleId && !rfm.IsDeleted)
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
