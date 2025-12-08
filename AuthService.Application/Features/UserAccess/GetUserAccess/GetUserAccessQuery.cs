namespace AuthService.Application.Features.UserAccess.GetUserAccess;

// ============================================================
//  UPDATED: Added PagePermissions dictionary
// Each page now has its own specific permission list
// ============================================================

/// <summary>
/// Query to get user access details
/// </summary>
public sealed record GetUserAccessQuery(Guid UserId) : IRequest<UserAccessDto>;
public sealed record UserAccessDto(
    Guid UserId,
    string Email,
    List<string> Roles,
    List<string> Permissions,  // Legacy: All unique permissions (for backward compatibility)
    List<PageAccessDto> PageAccess,
    Dictionary<string, List<string>> PagePermissions,  //  NEW: Page-specific permissions
    Guid? DepartmentId,  //  ADDED: User's department ID
    string? DepartmentName  //  ADDED: User's department name
)
{
    //  Helper method: Get permissions for specific page
    public List<string> GetPermissionsForPage(string pageName)
    {
        if (PagePermissions.TryGetValue(pageName, out var permissions))
        {
            return permissions;
        }
        return new List<string>();
    }

    //  Helper method: Check if user has specific permission on specific page
    public bool HasPermissionOnPage(string pageName, string permission)
    {
        var pagePerms = GetPermissionsForPage(pageName);
        return pagePerms.Any(p => p.Equals(permission, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record PageAccessDto(
    Guid PageId,
    string PageName,
    string PageUrl,
    List<string> Features
);

// ============================================================
// EXAMPLE USAGE:
//
// var userAccess = await GetUserAccessByEmail("financeintern@authmanagement.com");
//
// // Get permissions for Test Categories page
// var testCategoriesPerms = userAccess.GetPermissionsForPage("Test Categories");
// // Returns: ["View"] ← ONLY View
//
// // Check if user can edit Test Categories
// var canEdit = userAccess.HasPermissionOnPage("Test Categories", "Update");
// // Returns: false ← Correct!
//
// // Check if user can edit Change Password
// var canEditPassword = userAccess.HasPermissionOnPage("Change Password", "Update");
// // Returns: true ← Different page, different permission!
// ============================================================
