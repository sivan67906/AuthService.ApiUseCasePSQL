namespace AuthService.Domain.Constants;

/// <summary>
/// Department-specific and functional roles
/// System roles (SuperAdmin, DepartmentAdmin, PendingUser) are in SystemRoles.cs
/// </summary>
public static class Roles
{
    // Admin Roles - Full CRUD within their department
    public const string Admin = "Admin";
    public const string FinanceAdmin = "FinanceAdmin";
    public const string HRAdmin = "HRAdmin";
    public const string ITAdmin = "ITAdmin";
    
    // Manager Roles - Full CRUD except Delete within their department
    public const string Manager = "Manager";
    public const string FinanceManager = "FinanceManager";
    public const string HRManager = "HRManager";
    public const string ITManager = "ITManager";
    
    // Supervisor Roles - Create, View, Update (no Delete)
    public const string FinanceSupervisor = "FinanceSupervisor";
    public const string MarketingSupervisor = "MarketingSupervisor";
    
    // Analyst/Executive Roles - View + Create only (no Edit/Delete)
    public const string Analyst = "Analyst";
    public const string Executive = "Executive";
    public const string FinanceAnalyst = "FinanceAnalyst";
    public const string HRAnalyst = "HRAnalyst";
    
    // Staff Roles - Create and View
    public const string Staff = "Staff";
    public const string FinanceStaff = "FinanceStaff";
    public const string HRStaff = "HRStaff";
    public const string MarketingStaff = "MarketingStaff";
    
    // Intern Roles - View only
    public const string FinanceIntern = "FinanceIntern";
    public const string MarketingIntern = "MarketingIntern";
    
    // Marketing Roles
    public const string MarketingManager = "MarketingManager";
    
    // Legacy/Other Roles
    public const string User = "User";
    public const string Accountant = "Accountant";
    public const string Auditor = "Auditor";
    
    /// <summary>
    /// Get all department-specific roles (excludes system roles)
    /// </summary>
    public static string[] GetAllDepartmentRoles() => new[]
    {
        FinanceManager, FinanceSupervisor, FinanceStaff, FinanceIntern,
        MarketingManager, MarketingSupervisor, MarketingStaff, MarketingIntern
    };
}
