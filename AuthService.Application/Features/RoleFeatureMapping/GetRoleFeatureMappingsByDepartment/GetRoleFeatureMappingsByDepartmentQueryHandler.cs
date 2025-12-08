using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingsByDepartment;

public sealed class GetRoleFeatureMappingsByDepartmentQueryHandler : IRequestHandler<GetRoleFeatureMappingsByDepartmentQuery, List<RoleFeatureMappingDto>>
{
    private readonly IAppDbContext _db;

    public GetRoleFeatureMappingsByDepartmentQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoleFeatureMappingDto>> Handle(GetRoleFeatureMappingsByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var mappings = await _db.RoleFeatureMappings.AsNoTracking()
            .Include(rfm => rfm.Role)
            .Include(rfm => rfm.Feature)
            .Include(rfm => rfm.Department)
            .Where(rfm => rfm.DepartmentId == request.DepartmentId && !rfm.IsDeleted)
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
