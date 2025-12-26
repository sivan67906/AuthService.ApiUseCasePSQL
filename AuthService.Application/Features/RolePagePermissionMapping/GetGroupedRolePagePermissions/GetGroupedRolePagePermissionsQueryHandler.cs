using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetGroupedRolePagePermissions;

public class GetGroupedRolePagePermissionsQueryHandler
    : IRequestHandler<GetGroupedRolePagePermissionsQuery, List<RolePagePermissionGroupDto>>
{
    private readonly IAppDbContext _db;

    public GetGroupedRolePagePermissionsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RolePagePermissionGroupDto>> Handle(
        GetGroupedRolePagePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var mappings = await _db.RolePagePermissionMappings
            .Include(m => m.Department)
            .Include(m => m.Role)
            .Include(m => m.Page)
            .Include(m => m.Permission)
            .Where(m => !m.IsDeleted)
            .ToListAsync(cancellationToken);

        // Group by Department, Role, Page ONLY (not by CreatedAt or other properties)
        var grouped = mappings
            .GroupBy(m => new
            {
                m.DepartmentId,
                m.RoleId,
                m.PageId
            })
            .Select(g =>
            {
                var first = g.First();
                return new RolePagePermissionGroupDto
                {
                    DepartmentId = g.Key.DepartmentId,
                    DepartmentName = first.Department?.Name ?? "System-wide",
                    RoleId = g.Key.RoleId,
                    RoleName = first.Role?.Name ?? string.Empty,
                    PageId = g.Key.PageId,
                    PageName = first.Page?.Name ?? string.Empty,
                    PageUrl = first.Page?.Url ?? string.Empty,
                    CreatedAt = g.Min(m => m.CreatedAt), // Use earliest CreatedAt from the group
                    Permissions = g.Select(m => new PermissionBadgeDto
                    {
                        Id = m.Id,
                        PermissionId = m.PermissionId,
                        PermissionName = m.Permission.Name,
                        PermissionCode = GetPermissionCode(m.Permission.Name),
                        BadgeColor = GetBadgeColor(m.Permission.Name)
                    })
                    .OrderBy(p => GetPermissionOrder(p.PermissionName))
                    .ToList()
                };
            })
            .OrderBy(g => g.DepartmentName)
            .ThenBy(g => g.RoleName)
            .ThenBy(g => g.PageName)
            .ToList();

        return grouped;
    }

    private static string GetPermissionCode(string permissionName)
    {
        return permissionName.ToLower() switch
        {
            "view" => "V",
            "create" => "C",
            "edit" => "E",
            "delete" => "D",
            _ => permissionName.Length > 0 ? permissionName[..1].ToUpper() : "?"
        };
    }

    private static string GetBadgeColor(string permissionName)
    {
        return permissionName.ToLower() switch
        {
            "view" => "primary",
            "create" => "success",
            "edit" => "warning",
            "delete" => "danger",
            _ => "secondary"
        };
    }

    private static int GetPermissionOrder(string permissionName)
    {
        return permissionName.ToLower() switch
        {
            "view" => 1,
            "create" => 2,
            "update" => 3,
            "edit" => 3,
            "delete" => 4,
            _ => 99
        };
    }
}
