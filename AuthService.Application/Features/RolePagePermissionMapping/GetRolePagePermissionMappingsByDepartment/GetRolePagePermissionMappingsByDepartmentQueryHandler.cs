using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByDepartment;

public sealed class GetRolePagePermissionMappingsByDepartmentQueryHandler : IRequestHandler<GetRolePagePermissionMappingsByDepartmentQuery, List<RolePagePermissionMappingDto>>
{
    private readonly IAppDbContext _db;

    public GetRolePagePermissionMappingsByDepartmentQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RolePagePermissionMappingDto>> Handle(GetRolePagePermissionMappingsByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var mappings = await _db.RolePagePermissionMappings.AsNoTracking()
            .Include(rppm => rppm.Role)
            .Include(rppm => rppm.Page)
            .Include(rppm => rppm.Permission)
            .Include(rppm => rppm.Department)
            .Where(rppm => rppm.DepartmentId == request.DepartmentId && !rppm.IsDeleted)
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
            .ToListAsync(cancellationToken);

        return mappings;
    }
}
