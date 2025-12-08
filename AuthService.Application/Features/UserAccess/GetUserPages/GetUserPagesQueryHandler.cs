using Microsoft.AspNetCore.Identity;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetUserPages;

public sealed class GetUserPagesQueryHandler : IRequestHandler<GetUserPagesQuery, List<UserPageDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppDbContext _db;

    public GetUserPagesQueryHandler(UserManager<ApplicationUser> userManager, IAppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<List<UserPageDto>> Handle(GetUserPagesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
        }

        // FIXED: Use UserRoleMappings instead of Identity UserRoles
        var userRoleIds = await _db.UserRoleMappings.AsNoTracking()
            .Where(urm => urm.UserId == user.Id && urm.IsActive)
            .Select(urm => urm.RoleId)
            .ToListAsync(cancellationToken);

        if (!userRoleIds.Any())
        {
            return new List<UserPageDto>();
        }

        // Get role names to check for SuperAdmin
        var roleNames = await _db.ApplicationRoles.AsNoTracking()
            .Where(r => userRoleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

        var isSuperAdmin = roleNames.Contains(SystemRoles.SuperAdmin);

        List<UserPageDto> result;

        if (isSuperAdmin)
        {
            // SuperAdmin gets ALL pages with ALL permissions
            var allPages = await _db.Pages.AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);

            var allPermissions = await _db.Permissions.AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);

            result = new List<UserPageDto>();

            foreach (var page in allPages)
            {
                // Get features for this page
                var pageFeatures = await _db.PageFeatureMappings.AsNoTracking()
                    .Where(pfm => pfm.PageId == page.Id && pfm.IsActive)
                    .Join(_db.Features,
                        pfm => pfm.FeatureId,
                        f => f.Id,
                        (pfm, f) => f.Name)
                    .ToListAsync(cancellationToken);

                result.Add(new UserPageDto
                {
                    PageId = page.Id,
                    Name = page.Name,
                    Url = page.Url,
                    Description = page.Description,
                    DisplayOrder = page.DisplayOrder,
                    RequiredPermissions = allPermissions, // SuperAdmin has all permissions
                    Features = pageFeatures
                });
            }
        }
        else
        {
            // Regular users - get permissions from their roles
            var permissionIds = await _db.RolePermissionMappings.AsNoTracking()
                .Where(rpm => userRoleIds.Contains(rpm.RoleId) && rpm.IsActive)
                .Select(rpm => rpm.PermissionId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!permissionIds.Any())
            {
                return new List<UserPageDto>();
            }

            // Get pages that require these permissions
            var pageIds = await _db.PagePermissionMappings.AsNoTracking()
                .Where(ppm => permissionIds.Contains(ppm.PermissionId) && ppm.IsActive)
                .Select(ppm => ppm.PageId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!pageIds.Any())
            {
                return new List<UserPageDto>();
            }

            // Get page details with their permissions and features
            var pages = await _db.Pages.AsNoTracking()
                .Where(p => pageIds.Contains(p.Id) && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);

            result = new List<UserPageDto>();

            foreach (var page in pages)
            {
                // Get permissions for this page
                var pagePermissions = await _db.PagePermissionMappings.AsNoTracking()
                    .Where(ppm => ppm.PageId == page.Id && ppm.IsActive)
                    .Join(_db.Permissions,
                        ppm => ppm.PermissionId,
                        p => p.Id,
                        (ppm, p) => p.Name)
                    .ToListAsync(cancellationToken);

                // Get features for this page
                var pageFeatures = await _db.PageFeatureMappings.AsNoTracking()
                    .Where(pfm => pfm.PageId == page.Id && pfm.IsActive)
                    .Join(_db.Features,
                        pfm => pfm.FeatureId,
                        f => f.Id,
                        (pfm, f) => f.Name)
                    .ToListAsync(cancellationToken);

                result.Add(new UserPageDto
                {
                    PageId = page.Id,
                    Name = page.Name,
                    Url = page.Url,
                    Description = page.Description,
                    DisplayOrder = page.DisplayOrder,
                    RequiredPermissions = pagePermissions,
                    Features = pageFeatures
                });
            }
        }

        return result.OrderBy(p => p.DisplayOrder).ToList();
    }
}