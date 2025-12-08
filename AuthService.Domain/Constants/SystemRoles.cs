namespace AuthService.Domain.Constants;

/// <summary>
/// System-level roles that are hardcoded and used for core authorization
/// All other roles are dynamic and managed through the database
/// </summary>
public static class SystemRoles
{
    // SuperAdmin - Has unrestricted access to everything across all departments
    public const string SuperAdmin = "SuperAdmin";
    
    // DepartmentAdmin - Has full access within their assigned department only
    public const string DepartmentAdmin = "DepartmentAdmin";
    
    // PendingUser - Newly registered users waiting for role assignment
    public const string PendingUser = "PendingUser";
    
    /// <summary>
    /// Get all system roles that should be created during initial setup
    /// </summary>
    public static string[] GetAll() => new[] { SuperAdmin, DepartmentAdmin, PendingUser };
    
    /// <summary>
    /// Check if a role name is a system role
    /// </summary>
    public static bool IsSystemRole(string roleName)
    {
        return roleName == SuperAdmin || roleName == DepartmentAdmin || roleName == PendingUser;
    }
}
