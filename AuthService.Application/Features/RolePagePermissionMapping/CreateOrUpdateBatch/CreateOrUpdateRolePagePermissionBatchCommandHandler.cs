using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.CreateOrUpdateBatch;

public class CreateOrUpdateRolePagePermissionBatchCommandHandler
    : IRequestHandler<CreateOrUpdateRolePagePermissionBatchCommand, List<RolePagePermissionMappingDto>>
{
    private readonly IAppDbContext _db;

    public CreateOrUpdateRolePagePermissionBatchCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RolePagePermissionMappingDto>> Handle(
        CreateOrUpdateRolePagePermissionBatchCommand request,
        CancellationToken cancellationToken)
    {
        // Validate that View permission is included
        var viewPermission = await _db.Permissions
            .FirstOrDefaultAsync(p => p.Name == "View", cancellationToken);

        if (viewPermission == null || !request.PermissionIds.Contains(viewPermission.Id))
        {
            throw new InvalidOperationException("View permission is mandatory and must be included");
        }

        // Get existing mappings
        var existingMappings = await _db.RolePagePermissionMappings
            .Include(m => m.Permission)
            .Where(m => !m.IsDeleted
                     && m.DepartmentId == request.DepartmentId
                     && m.RoleId == request.RoleId
                     && m.PageId == request.PageId)
            .ToListAsync(cancellationToken);

        // Remove mappings that are no longer in the list
        var toRemove = existingMappings
            .Where(m => !request.PermissionIds.Contains(m.PermissionId))
            .ToList();

        foreach (var mapping in toRemove)
        {
            // Check if removing View permission when others exist
            if (mapping.Permission.Name.Equals("View", StringComparison.OrdinalIgnoreCase))
            {
                var otherMappingsStillExist = existingMappings
                    .Any(m => m.Id != mapping.Id && request.PermissionIds.Contains(m.PermissionId));

                if (otherMappingsStillExist)
                {
                    throw new InvalidOperationException(
                        "Cannot remove View permission while other permissions exist");
                }
            }
            _db.RolePagePermissionMappings.Remove(mapping);
        }

        // Add new mappings
        var existingPermissionIds = existingMappings.Select(m => m.PermissionId).ToList();
        var toAdd = request.PermissionIds
            .Where(pid => !existingPermissionIds.Contains(pid))
            .ToList();

        foreach (var permissionId in toAdd)
        {
            var newMapping = new Domain.Entities.RolePagePermissionMapping
            {
                DepartmentId = request.DepartmentId,
                RoleId = request.RoleId,
                PageId = request.PageId,
                PermissionId = permissionId
            };
            _db.RolePagePermissionMappings.Add(newMapping);
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Return updated mappings
        var result = await _db.RolePagePermissionMappings
            .Include(m => m.Permission)
            .Include(m => m.Role)
            .Include(m => m.Page)
            .Include(m => m.Department)
            .Where(m => !m.IsDeleted
                     && m.DepartmentId == request.DepartmentId
                     && m.RoleId == request.RoleId
                     && m.PageId == request.PageId)
            .Select(m => new RolePagePermissionMappingDto
            {
                Id = m.Id,
                DepartmentId = m.DepartmentId,
                DepartmentName = m.Department != null ? m.Department.Name : null,
                RoleId = m.RoleId,
                RoleName = m.Role.Name!,
                PageId = m.PageId,
                PageName = m.Page.Name,
                PermissionId = m.PermissionId,
                PermissionName = m.Permission.Name,
                IsActive = true,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return result;
    }
}
