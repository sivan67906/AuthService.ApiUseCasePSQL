using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;

namespace AuthService.Application.Features.UserAccess.GetUserNavigation;

// Query
public record GetUserNavigationQuery(string UserId) : IRequest<UserNavigationDto>;

// DTOs
public class UserNavigationDto
{
    public List<MenuItemDto> MenuItems { get; set; } = new();
    public string UserEmail { get; set; } = string.Empty;
    public List<string> UserRoles { get; set; } = new();
    public string? DepartmentName { get; set; }
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMainMenu { get; set; }
    public List<SubMenuItemDto> SubMenus { get; set; } = new();
}

public class SubMenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<PageItemDto> Pages { get; set; } = new();
}

public class PageItemDto
{
    public Guid PageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<string> RequiredPermissions { get; set; } = new();
}

// Handler
public class GetUserNavigationHandler : IRequestHandler<GetUserNavigationQuery, UserNavigationDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppDbContext _db;

    public GetUserNavigationHandler(UserManager<ApplicationUser> userManager, IAppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<UserNavigationDto> Handle(GetUserNavigationQuery request, CancellationToken cancellationToken)
    {
        // Get user using UserManager
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new UserNavigationDto();
        }

        // Get user's roles using UserManager
        var userRoleNames = await _userManager.GetRolesAsync(user);
        if (!userRoleNames.Any())
        {
            return new UserNavigationDto
            {
                UserEmail = user.Email ?? string.Empty,
                UserRoles = new List<string>()
            };
        }

        // Check if user is SuperAdmin
        bool isSuperAdmin = userRoleNames.Contains(SystemRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase);

        // Get role IDs from role names
        var roles = await _db.ApplicationRoles
            .Where(r => userRoleNames.Contains(r.Name!))
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();

        // Get user's department (if any)
        var userDepartmentId = roles.FirstOrDefault(r => r.DepartmentId.HasValue)?.DepartmentId;
        string? userDepartmentName = null;

        if (userDepartmentId.HasValue)
        {
            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == userDepartmentId.Value, cancellationToken);
            userDepartmentName = department?.Name;
        }

        // Get accessible permissions based on roles
        var userPermissionIds = await _db.RolePermissionMappings
            .Where(rpm => roleIds.Contains(rpm.RoleId) && rpm.IsActive)
            .Select(rpm => rpm.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get all pages that are accessible to the user
        List<Domain.Entities.Page> accessiblePages;

        if (isSuperAdmin)
        {
            // SuperAdmin has access to ALL pages
            accessiblePages = await _db.Pages
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Get pages based on page-permission mappings and user's permissions
            if (!userPermissionIds.Any())
            {
                return new UserNavigationDto
                {
                    UserEmail = user.Email ?? string.Empty,
                    UserRoles = userRoleNames.ToList(),
                    DepartmentName = userDepartmentName
                };
            }

            var pageIdsFromPermissions = await _db.PagePermissionMappings
                .Where(ppm => userPermissionIds.Contains(ppm.PermissionId) && ppm.IsActive)
                .Select(ppm => ppm.PageId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!pageIdsFromPermissions.Any())
            {
                return new UserNavigationDto
                {
                    UserEmail = user.Email ?? string.Empty,
                    UserRoles = userRoleNames.ToList(),
                    DepartmentName = userDepartmentName
                };
            }

            accessiblePages = await _db.Pages
                .Where(p => pageIdsFromPermissions.Contains(p.Id) && p.IsActive)
                .ToListAsync(cancellationToken);
        }

        if (!accessiblePages.Any())
        {
            return new UserNavigationDto
            {
                UserEmail = user.Email ?? string.Empty,
                UserRoles = userRoleNames.ToList(),
                DepartmentName = userDepartmentName
            };
        }

        var accessiblePageIds = accessiblePages.Select(p => p.Id).ToList();

        // Get page-feature mappings for accessible pages
        var pageFeatureMappings = await _db.PageFeatureMappings
            .Where(pfm => accessiblePageIds.Contains(pfm.PageId) && pfm.IsActive)
            .ToListAsync(cancellationToken);

        // Get all features (including menus and submenus)
        var featureIds = pageFeatureMappings.Select(pfm => pfm.FeatureId).Distinct().ToList();
        var features = await _db.Features
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken);

        // Build hierarchical menu structure
        var mainMenus = features
            .Where(f => f.IsMainMenu && f.ParentFeatureId == null)
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new MenuItemDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Icon = f.Icon,
                DisplayOrder = f.DisplayOrder,
                IsMainMenu = f.IsMainMenu,
                SubMenus = new List<SubMenuItemDto>()
            })
            .ToList();

        // Build submenus under each main menu
        foreach (var mainMenu in mainMenus)
        {
            var subMenus = features
                .Where(f => !f.IsMainMenu && f.ParentFeatureId == mainMenu.Id)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new SubMenuItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Icon = f.Icon,
                    DisplayOrder = f.DisplayOrder,
                    Pages = new List<PageItemDto>()
                })
                .ToList();

            // Build pages under each submenu
            foreach (var subMenu in subMenus)
            {
                var pagesForSubMenu = pageFeatureMappings
                    .Where(pfm => pfm.FeatureId == subMenu.Id)
                    .Join(accessiblePages,
                        pfm => pfm.PageId,
                        p => p.Id,
                        (pfm, p) => p)
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new PageItemDto
                    {
                        PageId = p.Id,
                        Name = p.Name,
                        Url = p.Url,
                        Description = p.Description,
                        DisplayOrder = p.DisplayOrder,
                        RequiredPermissions = new List<string>()
                    })
                    .ToList();

                subMenu.Pages = pagesForSubMenu;
            }

            // Only include submenus that have pages
            mainMenu.SubMenus = subMenus.Where(sm => sm.Pages.Any()).ToList();
        }

        // Only include main menus that have submenus with pages
        var filteredMainMenus = mainMenus.Where(mm => mm.SubMenus.Any()).ToList();

        return new UserNavigationDto
        {
            MenuItems = filteredMainMenus,
            UserEmail = user.Email ?? string.Empty,
            UserRoles = userRoleNames.ToList(),
            DepartmentName = userDepartmentName
        };
    }
}
