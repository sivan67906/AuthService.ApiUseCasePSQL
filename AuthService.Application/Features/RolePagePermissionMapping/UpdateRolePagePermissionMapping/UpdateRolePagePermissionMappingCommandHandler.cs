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
        var mapping = await _db.RolePagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Role page permission mapping with ID {request.Id} not found");
        }

        mapping.RoleId = request.RoleId;
        mapping.PageId = request.PageId;
        mapping.PermissionId = request.PermissionId;
        mapping.DepartmentId = request.DepartmentId;
        mapping.IsActive = request.IsActive;
        mapping.UpdatedAt = DateTime.UtcNow;

        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.RolePagePermissionMapping>().Update(mapping);

        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdateRolePagePermissionMappingHandler] Saved {savedCount} entities for RolePagePermissionMapping ID: {request.Id}");

        var result = await _db.RolePagePermissionMappings
            .Include(rppm => rppm.Role)
            .Include(rppm => rppm.Page)
            .Include(rppm => rppm.Permission)
            .Include(rppm => rppm.Department)
            .Where(rppm => rppm.Id == request.Id)
            .Select(rppm => new RolePagePermissionMappingDto
            {
                Id = rppm.Id,
                RoleId = rppm.RoleId,
                RoleName = rppm.Role.Name ?? string.Empty,
                PageId = rppm.PageId,
                PageName = rppm.Page.Name,
                PermissionId = rppm.PermissionId,
                PermissionName = rppm.Permission.Name,
                DepartmentId = rppm.DepartmentId,
                DepartmentName = rppm.Department != null ? rppm.Department.Name : null,
                IsActive = rppm.IsActive,
                CreatedAt = rppm.CreatedAt,
                UpdatedAt = rppm.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result!;
    }
}
