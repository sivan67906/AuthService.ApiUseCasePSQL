using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetUserAccess;

// ============================================================
// This is the OTHER handler (without ByEmail)
// It also needs the PagePermissions parameter
// ============================================================

public sealed class GetUserAccessQueryHandler
    : IRequestHandler<GetUserAccessQuery, UserAccessDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;

    public GetUserAccessQueryHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IAppDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<UserAccessDto> Handle(GetUserAccessQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
        }

        // Get user roles and department from UserRoleMappings
        var userRoleData = await _db.UserRoleMappings.AsNoTracking()
            .Where(urm => urm.UserId == user.Id && urm.IsActive)
            .Select(urm => new { urm.RoleId, urm.DepartmentId })
            .ToListAsync(cancellationToken);

        if (!userRoleData.Any())
        {
            // Return empty access if no roles assigned (no department)
            return new UserAccessDto(
                user.Id,
                user.Email ?? string.Empty,
                new List<string>(),
                new List<string>(),
                new List<PageAccessDto>(),
                new Dictionary<string, List<string>>(),
                null,  // DepartmentId
                null   // DepartmentName
            );
        }

        var userRoleIds = userRoleData.Select(x => x.RoleId).Distinct().ToList();
        var userDepartmentId = userRoleData.FirstOrDefault()?.DepartmentId;

        // Get department name if user has a department
        string? userDepartmentName = null;
        if (userDepartmentId.HasValue)
        {
            userDepartmentName = await _db.Departments.AsNoTracking()
                .Where(d => d.Id == userDepartmentId.Value)
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
        var pagePermissions = new Dictionary<string, List<string>>();  //  ADDED

        // Check if SuperAdmin
        if (userRoles.Contains(SystemRoles.SuperAdmin))
        {
            var allPermissions = await _db.Permissions.AsNoTracking()
                .Where(p => p.IsActive && p.IsDeleted == false)
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);

            foreach (var perm in allPermissions)
            {
                permissions.Add(perm);
            }

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
                pagePermissions[page.Name] = allPermissions.ToList();  //  ADDED
            }
        }
        else
        {
            var rolePagePermissions = await _db.RolePagePermissionMappings.AsNoTracking()
                .Where(rppm => userRoleIds.Contains(rppm.RoleId)
                    && rppm.IsActive
                    && rppm.IsDeleted == false
                    && (rppm.DepartmentId == null || rppm.DepartmentId == userDepartmentId))
                .Include(rppm => rppm.Permission)
                .Include(rppm => rppm.Page)
                .ToListAsync(cancellationToken);

            //  Group permissions by page
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

                pagePermissions[pageName] = perms;

                foreach (var perm in perms)
                {
                    permissions.Add(perm);
                }
            }

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
            pagePermissions,
            userDepartmentId,      // DepartmentId
            userDepartmentName     // DepartmentName
        );
    }
}
