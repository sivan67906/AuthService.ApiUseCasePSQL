using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.CreateRolePagePermissionMapping;
public sealed class CreateRolePagePermissionMappingCommandHandler : IRequestHandler<CreateRolePagePermissionMappingCommand, RolePagePermissionMappingDto>
{
    private readonly IAppDbContext _db;
    public CreateRolePagePermissionMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<RolePagePermissionMappingDto> Handle(CreateRolePagePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        // Check if mapping already exists - including soft-deleted
        var existing = await _db.RolePagePermissionMappings
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && 
                          x.PageId == request.PageId && 
                          x.PermissionId == request.PermissionId &&
                          x.DepartmentId == request.DepartmentId, cancellationToken);
                          
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                throw new InvalidOperationException("This role-page-permission mapping already exists in deactivated mode. Please restore the deactivated mapping instead of creating a new one.");
            }
            else
            {
                throw new InvalidOperationException("This role-page-permission mapping already exists");
            }
        }
        
        var entity = new Domain.Entities.RolePagePermissionMapping
        {
            RoleId = request.RoleId,
            PageId = request.PageId,
            PermissionId = request.PermissionId,
            DepartmentId = request.DepartmentId,
            IsActive = request.IsActive
        };
        _db.RolePagePermissionMappings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<RolePagePermissionMappingDto>();
    }
}
