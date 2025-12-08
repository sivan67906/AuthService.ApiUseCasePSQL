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
        // Check for duplicate mapping
        var existingMapping = await _db.RolePagePermissionMappings
            .Where(rppm => rppm.RoleId == request.RoleId
                        && rppm.PageId == request.PageId
                        && rppm.PermissionId == request.PermissionId
                        && rppm.DepartmentId == request.DepartmentId
                        && !rppm.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingMapping != null)
        {
            throw new InvalidOperationException(
                $"A mapping already exists for this Role, Page, Permission, and Department combination. " +
                $"Please update the existing mapping or delete it before creating a new one.");
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

        var result = await _db.RolePagePermissionMappings
            .Include(rppm => rppm.Role)
            .Include(rppm => rppm.Page)
            .Include(rppm => rppm.Permission)
            .Include(rppm => rppm.Department)
            .Where(rppm => rppm.Id == entity.Id)
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
