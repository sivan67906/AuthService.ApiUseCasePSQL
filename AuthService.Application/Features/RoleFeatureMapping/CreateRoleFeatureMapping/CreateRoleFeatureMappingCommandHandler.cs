using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;

public sealed class CreateRoleFeatureMappingCommandHandler : IRequestHandler<CreateRoleFeatureMappingCommand, RoleFeatureMappingDto>
{
    private readonly IAppDbContext _db;

    public CreateRoleFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleFeatureMappingDto> Handle(CreateRoleFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.RoleFeatureMapping
        {
            RoleId = request.RoleId,
            FeatureId = request.FeatureId,
            DepartmentId = request.DepartmentId,
            IsActive = request.IsActive
        };

        _db.RoleFeatureMappings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var result = await _db.RoleFeatureMappings
            .Include(rfm => rfm.Role)
            .Include(rfm => rfm.Feature)
            .Include(rfm => rfm.Department)
            .Where(rfm => rfm.Id == entity.Id)
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

        return result!;
    }
}
