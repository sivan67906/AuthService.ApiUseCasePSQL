using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.UserAccess.GetUserAccess;
using AuthService.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetUserAccessByEmail;

public sealed class GetUserAccessByEmailQueryHandler
    : IRequestHandler<GetUserAccessByEmailQuery, UserAccessDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;

    public GetUserAccessByEmailQueryHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IAppDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<UserAccessDto> Handle(GetUserAccessByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' not found");
        }

        // Get user roles and department from UserRoleMappings
        var userRoleData = await _db.UserRoleMappings.AsNoTracking()
            .Where(urm => urm.UserId == user.Id && urm.IsActive)
            .Select(urm => new { urm.RoleId, urm.DepartmentId })
            .ToListAsync(cancellationToken);

        if (!userRoleData.Any())
        {
            // Return empty access if no roles assigned
            return new UserAccessDto(
                user.Id,
                user.Email ?? string.Empty,
                new List<string>(),
                new List<string>(),
                new List<PageAccessDto>(),
                new Dictionary<string, List<string>>(),  //  ADDED: Empty page permissions
                null,  //  No department
                null   //  No department name
            );
        }

        var userRoleIds = userRoleData.Select(x => x.RoleId).Distinct().ToList();
        var userDepartmentId = userRoleData.FirstOrDefault()?.DepartmentId;
        
        // Get department name
        string? userDepartmentName = null;
        if (userDepartmentId.HasValue)
        {
            userDepartmentName = await _db.Departments.AsNoTracking()
                .Where(d => d.Id == userDepartmentId.Value && !d.IsDeleted)
                .Select(d => d.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Get role names
        var userRoles = await _db.ApplicationRoles.AsNoTracking()
            .Where(r => userRoleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        var permissions = new HashSet<string>();
        var pageAccessList = new Dictionary<Guid, PageAccessDto>();
        var pagePermissions = new Dictionary<string, List<string>>();  //  ADDED: Page-specific permissions

        // Check if SuperAdmin - SuperAdmin has all permissions
        if (userRoles.Contains(SystemRoles.SuperAdmin))
        {
            // Get ALL permissions
            var allPermissions = await _db.Permissions.AsNoTracking()
                .Where(p => p.IsActive && p.IsDeleted == false)
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);

            foreach (var perm in allPermissions)
            {
                permissions.Add(perm);
            }

            // Get ALL pages
            var allPages = await _db.Pages.AsNoTracking()
                .Where(p => p.IsActive && p.IsDeleted == false)
                .ToListAsync(cancellationToken);

            foreach (var page in allPages)
            {
                var features = await _db.PageFeatureMappings.AsNoTracking()
                    .Where(pfm => pfm.PageId == page.Id && pfm.IsActive && pfm.IsDeleted == false)
                    .Include(pfm => pfm.Feature)
                    .Select(pfm => pfm.Feature.Name)
                    .ToListAsync(cancellationToken);

                var pageAccessDto = new PageAccessDto(
                    page.Id,
                    page.Name,
                    page.Url,
                    features
                );

                pageAccessList[page.Id] = pageAccessDto;

                //  ADDED: SuperAdmin has all permissions on all pages
                pagePermissions[page.Name] = allPermissions.ToList();
            }
        }
        else
        {
            //  FIXED: Use RolePagePermissionMappings with department filtering
            var rolePagePermissions = await _db.RolePagePermissionMappings.AsNoTracking()
                .Where(rppm => userRoleIds.Contains(rppm.RoleId)
                    && rppm.IsActive
                    && rppm.IsDeleted == false
                    && (rppm.DepartmentId == null || rppm.DepartmentId == userDepartmentId))
                .Include(rppm => rppm.Permission)
                .Include(rppm => rppm.Page)
                .ToListAsync(cancellationToken);

            // ============================================================
            //  KEY FIX: Group permissions by page name
            // Each page gets ONLY its own permissions, not mixed from all pages
            // ============================================================
            var groupedByPage = rolePagePermissions
                .Where(rppm => rppm.Page != null && rppm.Permission != null)
                .GroupBy(rppm => rppm.Page!.Name);

            foreach (var pageGroup in groupedByPage)
            {
                var pageName = pageGroup.Key;
                var perms = pageGroup
                    .Select(rppm => rppm.Permission!.Name)
                    .Distinct()
                    .ToList();

                // Store page-specific permissions
                pagePermissions[pageName] = perms;

                // Also collect all unique permissions for backward compatibility
                foreach (var perm in perms)
                {
                    permissions.Add(perm);
                }
            }

            // Build page access list from RolePagePermissionMappings
            var pageIdsWithAccess = rolePagePermissions
                .Where(rppm => rppm.Page != null)
                .Select(rppm => rppm.PageId)
                .Distinct()
                .ToList();

            var pages = await _db.Pages.AsNoTracking()
                .Where(p => pageIdsWithAccess.Contains(p.Id) && p.IsActive && p.IsDeleted == false)
                .ToListAsync(cancellationToken);

            foreach (var page in pages)
            {
                var features = await _db.PageFeatureMappings.AsNoTracking()
                    .Where(pfm => pfm.PageId == page.Id && pfm.IsActive && pfm.IsDeleted == false)
                    .Include(pfm => pfm.Feature)
                    .Select(pfm => pfm.Feature.Name)
                    .ToListAsync(cancellationToken);

                var pageAccessDto = new PageAccessDto(
                    page.Id,
                    page.Name,
                    page.Url,
                    features
                );

                pageAccessList[page.Id] = pageAccessDto;
            }
        }

        return new UserAccessDto(
            user.Id,
            user.Email ?? string.Empty,
            userRoles.ToList(),
            permissions.ToList(),
            pageAccessList.Values.ToList(),
            pagePermissions,  //  ADDED: Pass page-specific permissions
            userDepartmentId,  //  ADDED: User's department ID
            userDepartmentName  //  ADDED: User's department name
        );
    }
}

// ============================================================
// EXAMPLE OUTPUT FOR FinanceIntern:
// 
// pagePermissions = {
//   "Dashboard": ["View"],
//   "Profile": ["View"],
//   "Change Password": ["Update"],     // ← Has Update on this page
//   "User Addresses": ["View"],
//   "Test Categories": ["View"],       // ← ONLY View on this page
//   "Test Products": ["View"]          // ← ONLY View on this page
// }
//
// When Blazor requests "Test Categories", it gets ONLY ["View"]
// Not ["View", "Update"] mixed from all pages!
// ============================================================
