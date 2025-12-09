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
        var entity = await _db.RoleFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Role-Feature mapping with ID {request.Id} not found");
        }
        
        // Check if any changes were made
        bool hasChanges = false;
        if (entity.RoleId != request.RoleId || 
            entity.FeatureId != request.FeatureId || 
            entity.DepartmentId != request.DepartmentId ||
            entity.IsActive != request.IsActive)
        {
            hasChanges = true;
        }
        
        if (!hasChanges)
        {
            throw new InvalidOperationException("No changes detected. Please modify the data before updating.");
        }
        
        // Check for duplicate mapping (excluding current and soft-deleted)
        var duplicateExists = await _db.RoleFeatureMappings
            .Where(x => !x.IsDeleted && x.Id != request.Id)
            .AnyAsync(x => x.RoleId == request.RoleId && 
                          x.FeatureId == request.FeatureId && 
                          x.DepartmentId == request.DepartmentId, cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException("This role-feature mapping already exists");
        }
        
        entity.RoleId = request.RoleId;
        entity.FeatureId = request.FeatureId;
        entity.DepartmentId = request.DepartmentId;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        
        _db.Set<Domain.Entities.RoleFeatureMapping>().Update(entity);
        
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<RoleFeatureMappingDto>();
    }
}
