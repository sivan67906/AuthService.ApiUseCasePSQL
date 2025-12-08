using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.GetRoleFeatureMappingById;

public sealed class GetRoleFeatureMappingByIdQueryHandler : IRequestHandler<GetRoleFeatureMappingByIdQuery, RoleFeatureMappingDto>
{
    private readonly IAppDbContext _db;

    public GetRoleFeatureMappingByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleFeatureMappingDto> Handle(GetRoleFeatureMappingByIdQuery request, CancellationToken cancellationToken)
    {
        var mapping = await _db.RoleFeatureMappings.AsNoTracking()
            .Include(rfm => rfm.Role)
            .Include(rfm => rfm.Feature)
            .Include(rfm => rfm.Department)
            .Where(rfm => rfm.Id == request.Id && !rfm.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Role feature mapping with ID {request.Id} not found");
        }

        return mapping;
    }
}
