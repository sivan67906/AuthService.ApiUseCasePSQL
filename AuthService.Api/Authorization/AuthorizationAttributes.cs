using Microsoft.AspNetCore.Authorization;

namespace AuthService.Api.Authorization
{
    /// <summary>
    /// Custom authorization attribute that checks for specific roles.
    /// </summary>
    public class RoleAuthorizationAttribute : AuthorizeAttribute
    {
        public RoleAuthorizationAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }

    /// <summary>
    /// Custom authorization attribute for department-based access.
    /// Ensures user has access to a specific department.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class DepartmentAuthorizationAttribute : Attribute
    {
        public string? DepartmentName { get; }
        public bool AllowSuperAdmin { get; set; } = true;

        public DepartmentAuthorizationAttribute(string? departmentName = null)
        {
            DepartmentName = departmentName;
        }
    }

    /// <summary>
    /// Custom authorization attribute for permission-based access.
    /// Checks if user has a specific permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PermissionAuthorizationAttribute : Attribute
    {
        public string PermissionName { get; }

        public PermissionAuthorizationAttribute(string permissionName)
        {
            PermissionName = permissionName;
        }
    }

    /// <summary>
    /// Custom authorization attribute for page-based access.
    /// Checks if user has access to a specific page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PageAuthorizationAttribute : Attribute
    {
        public string PageName { get; }

        public PageAuthorizationAttribute(string pageName)
        {
            PageName = pageName;
        }
    }
}