using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.UpdateRolePagePermissionMapping;
public sealed class UpdateRolePagePermissionMappingCommandHandler : IRequestHandler<UpdateRolePagePermissionMappingCommand, RolePagePermissionMappingDto>
{
    private readonly IAppDbContext _db;
    public UpdateRolePagePermissionMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<RolePagePermissionMappingDto> Handle(UpdateRolePagePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.RolePagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Role-Page-Permission mapping with ID {request.Id} not found");
        }
        
        // Check if any changes were made
        bool hasChanges = false;
        if (entity.RoleId != request.RoleId || 
            entity.PageId != request.PageId || 
            entity.PermissionId != request.PermissionId ||
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
        var duplicateExists = await _db.RolePagePermissionMappings
            .Where(x => !x.IsDeleted && x.Id != request.Id)
            .AnyAsync(x => x.RoleId == request.RoleId && 
                          x.PageId == request.PageId && 
                          x.PermissionId == request.PermissionId &&
                          x.DepartmentId == request.DepartmentId, cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException("This role-page-permission mapping already exists");
        }
        
        entity.RoleId = request.RoleId;
        entity.PageId = request.PageId;
        entity.PermissionId = request.PermissionId;
        entity.DepartmentId = request.DepartmentId;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        
        _db.Set<Domain.Entities.RolePagePermissionMapping>().Update(entity);
        
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<RolePagePermissionMappingDto>();
    }
}
