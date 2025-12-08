using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.UpdateRoleFeatureMapping;

public sealed class UpdateRoleFeatureMappingCommandHandler : IRequestHandler<UpdateRoleFeatureMappingCommand, RoleFeatureMappingDto>
{
    private readonly IAppDbContext _db;

    public UpdateRoleFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleFeatureMappingDto> Handle(UpdateRoleFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _db.RoleFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Role feature mapping with ID {request.Id} not found");
        }

        mapping.RoleId = request.RoleId;
        mapping.FeatureId = request.FeatureId;
        mapping.DepartmentId = request.DepartmentId;
        mapping.IsActive = request.IsActive;
        mapping.UpdatedAt = DateTime.UtcNow;

        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.RoleFeatureMapping>().Update(mapping);

        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdateRoleFeatureMappingHandler] Saved {savedCount} entities for RoleFeatureMapping ID: {request.Id}");

        var result = await _db.RoleFeatureMappings
            .Include(rfm => rfm.Role)
            .Include(rfm => rfm.Feature)
            .Include(rfm => rfm.Department)
            .Where(rfm => rfm.Id == request.Id)
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
