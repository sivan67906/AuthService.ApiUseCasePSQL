using System.Diagnostics;
using System.Linq;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public interface IUserAuthorizationService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    Task<bool> UserHasAccessToPageAsync(Guid userId, string pageName);
    Task<bool> UserHasAccessToDepartmentAsync(Guid userId, Guid? departmentId);
    Task<List<MenuItemDto>> GetUserMenusAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<Guid?> GetUserDepartmentAsync(Guid userId);
    Task<List<string>> GetUserPagePermissionsAsync(Guid userId, string pageName);
}

public class UserAuthorizationService : IUserAuthorizationService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<UserAuthorizationService> _logger;

    public UserAuthorizationService(
        IAppDbContext db,
        ILogger<UserAuthorizationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
    {
        try
        {
            var userRoleData = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .Select(urm => new { urm.RoleId, urm.DepartmentId })
                .ToListAsync();

            if (userRoleData.Count == 0)
            {
                return false;
            }

            var userRoleIds = userRoleData.Select(x => x.RoleId).ToList();
            var userDepartmentId = userRoleData.FirstOrDefault()?.DepartmentId;

            var isSuperAdmin = await _db.ApplicationRoles
                .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == SystemRoles.SuperAdmin);

            if (isSuperAdmin)
            {
                return true;
            }

            // Check in RolePagePermissionMappings with department filtering
            var hasPermission = await _db.RolePagePermissionMappings
                .Where(rppm => userRoleIds.Contains(rppm.RoleId)
                    && rppm.IsActive
                    && (rppm.DepartmentId == null || rppm.DepartmentId == userDepartmentId))
                .Join(_db.Permissions,
                    rppm => rppm.PermissionId,
                    p => p.Id,
                    (rppm, p) => p.Name)
                .AnyAsync(name => name == permissionName);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionName} for user {UserId}", permissionName, userId);
            return false;
        }
    }

    public async Task<bool> UserHasAccessToPageAsync(Guid userId, string pageName)
    {
        try
        {
            var page = await _db.Pages
                .Where(p => p.Name == pageName && p.IsActive && p.IsDeleted == false)
                .FirstOrDefaultAsync();

            if (page == null)
            {
                return false;
            }

            var userRoleData = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .Select(urm => new { urm.RoleId, urm.DepartmentId })
                .ToListAsync();

            if (userRoleData.Count == 0)
            {
                return false;
            }

            var userRoleIds = userRoleData.Select(x => x.RoleId).ToList();
            var userDepartmentId = userRoleData.FirstOrDefault()?.DepartmentId;

            var isSuperAdmin = await _db.ApplicationRoles
                .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == SystemRoles.SuperAdmin);

            if (isSuperAdmin)
            {
                return true;
            }

            // Check if user has any permission on this page with department filtering
            var hasAccess = await _db.RolePagePermissionMappings
                .AnyAsync(rppm => userRoleIds.Contains(rppm.RoleId)
                    && rppm.PageId == page.Id
                    && rppm.IsActive
                    && rppm.IsDeleted == false
                    && (rppm.DepartmentId == null || rppm.DepartmentId == userDepartmentId));

            return hasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking page access for {PageName} for user {UserId}", pageName, userId);
            return false;
        }
    }

    public async Task<bool> UserHasAccessToDepartmentAsync(Guid userId, Guid? departmentId)
    {
        try
        {
            if (departmentId == null)
            {
                return true;
            }

            var userRoleMappings = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .ToListAsync();

            return userRoleMappings.Any(urm => urm.DepartmentId == departmentId || urm.DepartmentId == null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking department access for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Gets user menus based on RoleFeatureMappings - OPTIMIZED VERSION
    /// Only loads features and pages that the user actually has access to
    /// Applies department filtering for non-SuperAdmin users
    /// </summary>
    public async Task<List<MenuItemDto>> GetUserMenusAsync(Guid userId)
    {
        var totalStopwatch = Stopwatch.StartNew();
        _logger.LogInformation("=== MENU LOADING START for User: {UserId} (OPTIMIZED) ===", userId);

        try
        {
            // STEP 1: Get user roles and department
            var step1Stopwatch = Stopwatch.StartNew();
            var userRoleData = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .Select(urm => new { urm.RoleId, urm.DepartmentId })
                .ToListAsync();

            _logger.LogInformation("Step 1: User role data loaded in {Elapsed}ms - Found {Count} role mappings",
                step1Stopwatch.ElapsedMilliseconds, userRoleData.Count);

            if (userRoleData.Count == 0)
            {
                _logger.LogWarning("No role mappings found for user {UserId}", userId);
                return [];
            }

            var userRoleIds = userRoleData.Select(x => x.RoleId).Distinct().ToList();
            var userDepartmentId = userRoleData.FirstOrDefault()?.DepartmentId;

            // STEP 2: Check if SuperAdmin
            var step2Stopwatch = Stopwatch.StartNew();
            var roleNames = await _db.ApplicationRoles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            var isSuperAdmin = roleNames.Contains(SystemRoles.SuperAdmin);

            _logger.LogInformation("Step 2: Role check in {Elapsed}ms - IsSuperAdmin: {IsSuperAdmin}, Department: {DeptId}",
                step2Stopwatch.ElapsedMilliseconds, isSuperAdmin, userDepartmentId);

            // STEP 3: Get accessible features using RoleFeatureMappings with department filtering
            var step3Stopwatch = Stopwatch.StartNew();

            List<Guid> accessibleFeatureIds;
            if (isSuperAdmin)
            {
                // SuperAdmin: Get features mapped to SuperAdmin role (DepartmentId is NULL)
                accessibleFeatureIds = await _db.RoleFeatureMappings
                    .Where(rfm => userRoleIds.Contains(rfm.RoleId)
                        && rfm.IsActive
                        && rfm.IsDeleted == false
                        && rfm.DepartmentId == null)  // SuperAdmin has NULL department
                    .Select(rfm => rfm.FeatureId)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                // Regular users: Get features filtered by role AND department
                // Include features with matching department OR NULL (system-wide features)
                accessibleFeatureIds = await _db.RoleFeatureMappings
                    .Where(rfm => userRoleIds.Contains(rfm.RoleId)
                        && rfm.IsActive
                        && rfm.IsDeleted == false
                        && (rfm.DepartmentId == userDepartmentId || rfm.DepartmentId == null))  // Match user's department OR system-wide
                    .Select(rfm => rfm.FeatureId)
                    .Distinct()
                    .ToListAsync();
            }

            _logger.LogInformation("Step 3: Accessible features loaded in {Elapsed}ms - Found {Count} features for user",
                step3Stopwatch.ElapsedMilliseconds, accessibleFeatureIds.Count);

            if (accessibleFeatureIds.Count == 0)
            {
                _logger.LogWarning("No accessible features found for user {UserId} in department {DeptId}",
                    userId, userDepartmentId);
                return [];
            }

            // STEP 4: Load ONLY accessible features (not all features)
            var step4Stopwatch = Stopwatch.StartNew();
            var accessibleFeatures = await _db.Features
                .Where(f => accessibleFeatureIds.Contains(f.Id)
                    && f.IsActive
                    && f.IsDeleted == false)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Step 4: Feature details loaded in {Elapsed}ms - {Count} features",
                step4Stopwatch.ElapsedMilliseconds, accessibleFeatures.Count);

            // STEP 5: Get pages mapped to these features
            var step5Stopwatch = Stopwatch.StartNew();
            var pageFeatureMappings = await _db.PageFeatureMappings
                .Where(pfm => accessibleFeatureIds.Contains(pfm.FeatureId)
                    && pfm.IsActive
                    && pfm.IsDeleted == false)
                .AsNoTracking()
                .ToListAsync();

            var mappedPageIds = pageFeatureMappings.Select(pfm => pfm.PageId).Distinct().ToList();

            _logger.LogInformation("Step 5: Page-Feature mappings loaded in {Elapsed}ms - {Count} page mappings",
                step5Stopwatch.ElapsedMilliseconds, pageFeatureMappings.Count);

            // STEP 6: Get accessible pages with permission check
            var step6Stopwatch = Stopwatch.StartNew();

            List<Guid> accessiblePageIds;
            if (isSuperAdmin)
            {
                // SuperAdmin sees all mapped pages
                accessiblePageIds = mappedPageIds;
            }
            else
            {
                // Filter pages by RolePagePermissionMappings with department filtering
                accessiblePageIds = await _db.RolePagePermissionMappings
                    .Where(rppm => userRoleIds.Contains(rppm.RoleId)
                        && mappedPageIds.Contains(rppm.PageId)
                        && rppm.IsActive
                        && rppm.IsDeleted == false
                        && (rppm.DepartmentId == null || rppm.DepartmentId == userDepartmentId))
                    .Select(rppm => rppm.PageId)
                    .Distinct()
                    .ToListAsync();
            }

            var accessiblePages = await _db.Pages
                .Where(p => accessiblePageIds.Contains(p.Id)
                    && p.IsActive
                    && p.IsDeleted == false)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Step 6: Accessible pages loaded in {Elapsed}ms - {Count} pages",
                step6Stopwatch.ElapsedMilliseconds, accessiblePages.Count);

            // STEP 7: Build menu hierarchy
            var step7Stopwatch = Stopwatch.StartNew();
            var menuItems = BuildMenuHierarchy(accessibleFeatures, accessiblePages, pageFeatureMappings, accessiblePageIds.ToHashSet());
            step7Stopwatch.Stop();

            _logger.LogInformation("Step 7: Menu hierarchy built in {Elapsed}ms - {Count} top-level menus",
                step7Stopwatch.ElapsedMilliseconds, menuItems.Count);

            totalStopwatch.Stop();
            _logger.LogInformation("=== MENU LOADING COMPLETE: TOTAL TIME = {Elapsed}ms ===",
                totalStopwatch.ElapsedMilliseconds);

            return menuItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!!! ERROR loading menu after {Elapsed}ms: {Message}",
                totalStopwatch.ElapsedMilliseconds, ex.Message);
            return [];
        }
    }

    private List<MenuItemDto> BuildMenuHierarchy(
        List<Feature> accessibleFeatures,
        List<Page> accessiblePages,
        List<PageFeatureMapping> pageFeatureMappings,
        HashSet<Guid> accessiblePageIds)
    {
        var menuItems = new List<MenuItemDto>();

        try
        {
            // Get main menus (top level) from accessible features only
            var mainMenus = accessibleFeatures
                .Where(f => f.IsMainMenu && f.ParentFeatureId == null)
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            _logger.LogInformation("Building hierarchy from {Count} main menus", mainMenus.Count);

            foreach (var mainMenu in mainMenus)
            {
                var menuItem = BuildMenuItemRecursive(mainMenu, accessibleFeatures, accessiblePages,
                    pageFeatureMappings, accessiblePageIds);

                if (menuItem != null && (menuItem.SubMenus.Count > 0 || menuItem.Pages.Count > 0))
                {
                    menuItems.Add(menuItem);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building menu hierarchy: {Message}", ex.Message);
        }

        return menuItems;
    }

    private MenuItemDto? BuildMenuItemRecursive(
        Feature feature,
        List<Feature> accessibleFeatures,
        List<Page> accessiblePages,
        List<PageFeatureMapping> pageFeatureMappings,
        HashSet<Guid> accessiblePageIds)
    {
        try
        {
            var menuItem = new MenuItemDto
            {
                Id = feature.Id,
                Name = feature.Name ?? string.Empty,
                Description = feature.Description,
                Icon = feature.Icon,
                DisplayOrder = feature.DisplayOrder,
                Level = feature.Level,
                SubMenus = [],
                Pages = []
            };

            // Get child features from accessible features only
            var childFeatures = accessibleFeatures
                .Where(f => f.ParentFeatureId == feature.Id)
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            foreach (var childFeature in childFeatures)
            {
                var subMenuItem = BuildMenuItemRecursive(childFeature, accessibleFeatures, accessiblePages,
                    pageFeatureMappings, accessiblePageIds);

                if (subMenuItem != null && (subMenuItem.SubMenus.Count > 0 || subMenuItem.Pages.Count > 0))
                {
                    menuItem.SubMenus.Add(subMenuItem);
                }
            }

            // Get pages for this feature from accessible pages only
            var featurePageIds = pageFeatureMappings
                .Where(pfm => pfm.FeatureId == feature.Id)
                .Select(pfm => pfm.PageId)
                .ToHashSet();

            var featurePages = accessiblePages
                .Where(p => featurePageIds.Contains(p.Id) && accessiblePageIds.Contains(p.Id))
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new PageDto
                {
                    Id = p.Id,
                    PageId = p.Id,
                    Name = p.Name ?? string.Empty,
                    Url = p.Url ?? string.Empty,
                    Description = p.Description,
                    DisplayOrder = p.DisplayOrder,
                    ApiEndpoint = p.ApiEndpoint,
                    HttpMethod = p.HttpMethod
                })
                .ToList();

            menuItem.Pages = featurePages;
            return menuItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building menu item for feature {FeatureId}: {Message}", feature.Id, ex.Message);
            return null;
        }
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var roles = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .Join(_db.ApplicationRoles,
                    urm => urm.RoleId,
                    r => r.Id,
                    (urm, r) => r.Name!)
                .ToListAsync();

            return roles ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles for user {UserId}", userId);
            return [];
        }
    }

    public async Task<Guid?> GetUserDepartmentAsync(Guid userId)
    {
        try
        {
            var userRoleMapping = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .OrderBy(urm => urm.CreatedAt)
                .FirstOrDefaultAsync();

            return userRoleMapping?.DepartmentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department for user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Gets all permissions that a user has for a specific page.
    /// Returns permission names like ["View", "Create", "Update", "Delete"].
    /// Applies department filtering for non-SuperAdmin users.
    /// </summary>
    public async Task<List<string>> GetUserPagePermissionsAsync(Guid userId, string pageName)
    {
        try
        {
            // Get the page
            var page = await _db.Pages
                .Where(p => p.Name == pageName && p.IsActive && p.IsDeleted == false)
                .FirstOrDefaultAsync();

            if (page == null)
            {
                _logger.LogWarning("Page {PageName} not found", pageName);
                return [];
            }

            // Get user role data
            var userRoleData = await _db.UserRoleMappings
                .Where(urm => urm.UserId == userId && urm.IsActive)
                .Select(urm => new { urm.RoleId, urm.DepartmentId })
                .ToListAsync();

            if (userRoleData.Count == 0)
            {
                _logger.LogWarning("No role mappings found for user {UserId}", userId);
                return [];
            }

            var userRoleIds = userRoleData.Select(x => x.RoleId).ToList();
            var userDepartmentId = userRoleData.FirstOrDefault()?.DepartmentId;

            // Check if SuperAdmin
            var isSuperAdmin = await _db.ApplicationRoles
                .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == SystemRoles.SuperAdmin);

            // Get permissions
            List<string> permissions;
            if (isSuperAdmin)
            {
                // SuperAdmin gets all permissions for the page
                permissions = await _db.RolePagePermissionMappings
                    .Where(rppm => userRoleIds.Contains(rppm.RoleId)
                        && rppm.PageId == page.Id
                        && rppm.IsActive
                        && rppm.IsDeleted == false
                        && rppm.DepartmentId == null)  // SuperAdmin mappings have NULL department
                    .Join(_db.Permissions,
                        rppm => rppm.PermissionId,
                        p => p.Id,
                        (rppm, p) => p.Name!)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                // Regular users get permissions filtered by department
                permissions = await _db.RolePagePermissionMappings
                    .Where(rppm => userRoleIds.Contains(rppm.RoleId)
                        && rppm.PageId == page.Id
                        && rppm.IsActive
                        && rppm.IsDeleted == false
                        && (rppm.DepartmentId == null || rppm.DepartmentId == userDepartmentId))
                    .Join(_db.Permissions,
                        rppm => rppm.PermissionId,
                        p => p.Id,
                        (rppm, p) => p.Name!)
                    .Distinct()
                    .ToListAsync();
            }

            return permissions ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving page permissions for page {PageName} and user {UserId}", pageName, userId);
            return [];
        }
    }
}

// Public DTOs
public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public List<MenuItemDto> SubMenus { get; set; } = [];
    public List<PageDto> Pages { get; set; } = [];
}

public class PageDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? HttpMethod { get; set; }
}
