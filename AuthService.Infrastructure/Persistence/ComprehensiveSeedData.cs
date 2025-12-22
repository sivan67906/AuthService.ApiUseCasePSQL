using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Entities.Masters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Comprehensive seed data for RBAC system
/// Creates:
/// - Departments (Finance, Marketing)
/// - System Roles (SuperAdmin, DepartmentAdmin, PendingUser)
/// - Department Roles (FinanceManager, FinanceSupervisor, FinanceStaff, FinanceIntern, etc.)
/// - 20 Test Users with proper UserRoleMappings and EmailConfirmed
/// - Complete RBAC structure with Features (menus), Pages, RoleFeatureMapping, RolePagePermissionMapping
/// - RoleHierarchy for departments
/// - Permissions (Create, View, Update, Delete)
/// 
/// Menu Structure (CORRECT HIERARCHY):
/// 1. Dashboard (Main Menu) → /dashboard (direct page)
/// 2. RBAC Management (Main Menu) → pages directly: /department, /role, /feature, /page, /permission
/// 3. Mappings (Main Menu) → pages directly: /rolehierarchymapping, /userrolemapping, /rolefeaturemapping, /pagefeaturemapping, /rolepagepermissionmapping
/// 4. Account (Main Menu) → pages directly: /profile, /change-password
/// 5. Finance Management (Main Menu) → Company (SubMenu) → /company, /testcategories, /testproducts
/// 
/// Permission Matrix for Finance/Marketing Department Roles (5 pages: /profile, /change-password, /company, /testcategories, /testproducts):
/// - Manager: All permissions (Create, View, Update, Delete)
/// - Supervisor: No Delete permission
/// - Staff: View and Create only
/// - Intern: View only
/// </summary>
public static class ComprehensiveSeedData
{
    // Fixed GUIDs for consistency
    private static class FixedGuids
    {
        // System Roles
        public static readonly Guid SuperAdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid DepartmentAdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid PendingUserRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Departments
        public static readonly Guid FinanceDeptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid MarketingDeptId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Finance Roles
        public static readonly Guid FinanceManagerRoleId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid FinanceSupervisorRoleId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid FinanceStaffRoleId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public static readonly Guid FinanceInternRoleId = Guid.Parse("10000000-0000-0000-0000-000000000005");

        // Marketing Roles
        public static readonly Guid MarketingManagerRoleId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid MarketingSupervisorRoleId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid MarketingStaffRoleId = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public static readonly Guid MarketingInternRoleId = Guid.Parse("20000000-0000-0000-0000-000000000005");

        // Users - SuperAdmin
        public static readonly Guid SuperAdminUserId = Guid.Parse("99999999-9999-9999-9999-000000000001");
        
        // Users - Finance Department (6 users)
        public static readonly Guid FinanceAdminUserId = Guid.Parse("99999999-9999-9999-9999-000000000002");
        public static readonly Guid FinanceManagerUserId = Guid.Parse("99999999-9999-9999-9999-000000000003");
        public static readonly Guid FinanceSupervisorUserId = Guid.Parse("99999999-9999-9999-9999-000000000004");
        public static readonly Guid FinanceStaffUserId = Guid.Parse("99999999-9999-9999-9999-000000000005");
        public static readonly Guid FinanceInternUserId = Guid.Parse("99999999-9999-9999-9999-000000000006");
        public static readonly Guid FinanceStaff2UserId = Guid.Parse("99999999-9999-9999-9999-000000000007");
        
        // Users - Marketing Department (6 users)
        public static readonly Guid MarketingAdminUserId = Guid.Parse("99999999-9999-9999-9999-000000000008");
        public static readonly Guid MarketingManagerUserId = Guid.Parse("99999999-9999-9999-9999-000000000009");
        public static readonly Guid MarketingSupervisorUserId = Guid.Parse("99999999-9999-9999-9999-000000000010");
        public static readonly Guid MarketingStaffUserId = Guid.Parse("99999999-9999-9999-9999-000000000011");
        public static readonly Guid MarketingInternUserId = Guid.Parse("99999999-9999-9999-9999-000000000012");
        public static readonly Guid MarketingStaff2UserId = Guid.Parse("99999999-9999-9999-9999-000000000013");
        
        // Additional Users (7 more to make 20 total)
        public static readonly Guid FinanceIntern2UserId = Guid.Parse("99999999-9999-9999-9999-000000000014");
        public static readonly Guid FinanceSupervisor2UserId = Guid.Parse("99999999-9999-9999-9999-000000000015");
        public static readonly Guid MarketingIntern2UserId = Guid.Parse("99999999-9999-9999-9999-000000000016");
        public static readonly Guid MarketingSupervisor2UserId = Guid.Parse("99999999-9999-9999-9999-000000000017");
        public static readonly Guid FinanceManager2UserId = Guid.Parse("99999999-9999-9999-9999-000000000018");
        public static readonly Guid MarketingManager2UserId = Guid.Parse("99999999-9999-9999-9999-000000000019");
        public static readonly Guid SuperAdmin2UserId = Guid.Parse("99999999-9999-9999-9999-000000000020");

        // Fixed Feature IDs for consistency
        public static readonly Guid DashboardFeatureId = Guid.Parse("F0000000-0000-0000-0000-000000000001");
        public static readonly Guid RbacManagementFeatureId = Guid.Parse("F0000000-0000-0000-0000-000000000002");
        public static readonly Guid MappingsFeatureId = Guid.Parse("F0000000-0000-0000-0000-000000000003");
        public static readonly Guid AccountSettingsFeatureId = Guid.Parse("F0000000-0000-0000-0000-000000000004");
        public static readonly Guid FinanceManagementFeatureId = Guid.Parse("F0000000-0000-0000-0000-000000000005");
        public static readonly Guid CompanySubMenuFeatureId = Guid.Parse("F0000000-0000-0000-0000-000000000006");

        // Fixed Page IDs
        public static readonly Guid DashboardPageId = Guid.Parse("A0000000-0000-0000-0000-000000000001");
        public static readonly Guid DepartmentPageId = Guid.Parse("A0000000-0000-0000-0000-000000000002");
        public static readonly Guid RolePageId = Guid.Parse("A0000000-0000-0000-0000-000000000003");
        public static readonly Guid FeaturePageId = Guid.Parse("A0000000-0000-0000-0000-000000000004");
        public static readonly Guid PagePageId = Guid.Parse("A0000000-0000-0000-0000-000000000005");
        public static readonly Guid PermissionPageId = Guid.Parse("A0000000-0000-0000-0000-000000000006");
        public static readonly Guid RoleHierarchyMappingPageId = Guid.Parse("A0000000-0000-0000-0000-000000000007");
        public static readonly Guid UserRoleMappingPageId = Guid.Parse("A0000000-0000-0000-0000-000000000008");
        public static readonly Guid RoleFeatureMappingPageId = Guid.Parse("A0000000-0000-0000-0000-000000000009");
        public static readonly Guid PageFeatureMappingPageId = Guid.Parse("A0000000-0000-0000-0000-000000000010");
        public static readonly Guid RolePagePermissionMappingPageId = Guid.Parse("A0000000-0000-0000-0000-000000000011");
        public static readonly Guid ProfilePageId = Guid.Parse("A0000000-0000-0000-0000-000000000012");
        public static readonly Guid ChangePasswordPageId = Guid.Parse("A0000000-0000-0000-0000-000000000013");
        public static readonly Guid CompanyPageId = Guid.Parse("A0000000-0000-0000-0000-000000000014");
        public static readonly Guid TestCategoriesPageId = Guid.Parse("A0000000-0000-0000-0000-000000000015");
        public static readonly Guid TestProductsPageId = Guid.Parse("A0000000-0000-0000-0000-000000000016");

        // Fixed Permission IDs
        public static readonly Guid CreatePermissionId = Guid.Parse("B0000000-0000-0000-0000-000000000001");
        public static readonly Guid ViewPermissionId = Guid.Parse("B0000000-0000-0000-0000-000000000002");
        public static readonly Guid UpdatePermissionId = Guid.Parse("B0000000-0000-0000-0000-000000000003");
        public static readonly Guid DeletePermissionId = Guid.Parse("B0000000-0000-0000-0000-000000000004");
    }

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Check if data already exists
            if (await context.Permissions.AnyAsync())
            {
                logger.LogInformation("Database already seeded, skipping seed data");
                return;
            }

            logger.LogInformation("Starting comprehensive database seeding...");

            // 1. Create Permissions
            var permissions = await SeedPermissions(context, logger);

            // 2. Create Departments
            var departments = await SeedDepartments(context, logger);

            // 3. Create System Roles
            var systemRoles = await SeedSystemRoles(roleManager, logger);

            // 4. Create Department-specific Roles
            var departmentRoles = await SeedDepartmentRoles(roleManager, departments, logger);

            // 5. Create Features (Menu structure) - CORRECTED HIERARCHY
            var features = await SeedFeatures(context, logger);

            // 6. Create Pages
            var pages = await SeedPages(context, logger);

            // 7. Create Page-Feature Mappings - CORRECTED to map pages directly to main menus
            await SeedPageFeatureMappings(context, features, pages, logger);

            // 8. Create Role-Feature Mappings (with department scope)
            await SeedRoleFeatureMappings(context, systemRoles, departmentRoles, features, departments, logger);

            // 9. Create Role-Page-Permission Mappings (with department scope)
            await SeedRolePagePermissionMappings(context, systemRoles, departmentRoles, pages, permissions, departments, logger);

            // 10. Create Role Hierarchies
            await SeedRoleHierarchies(context, departmentRoles, departments, logger);

            // 11. Create CountryTimeZones mappings (only if Countries and TimeZones exist)
            await SeedCountryTimeZones(context, logger);

            // 12. Create 20 Test Users
            await SeedUsers(userManager, context, systemRoles, departmentRoles, departments, logger);

            logger.LogInformation("Comprehensive database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task<Dictionary<string, Guid>> SeedPermissions(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating permissions...");
        var permissions = new Dictionary<string, Guid>();

        var permissionList = new[]
        {
            (FixedGuids.CreatePermissionId, "CREATE", "Create", "Permission to create new records"),
            (FixedGuids.ViewPermissionId, "VIEW", "View", "Permission to view records"),
            (FixedGuids.UpdatePermissionId, "UPDATE", "Update", "Permission to update existing records"),
            (FixedGuids.DeletePermissionId, "DELETE", "Delete", "Permission to delete records")
        };

        foreach (var (id, code, name, description) in permissionList)
        {
            permissions[name] = id;
            context.Permissions.Add(new Permission
            {
                Id = id,
                Code = code,
                Name = name,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} permissions", permissions.Count);
        return permissions;
    }

    private static async Task<Dictionary<string, Guid>> SeedDepartments(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating departments...");
        var departments = new Dictionary<string, Guid>();

        var deptList = new[]
        {
            (FixedGuids.FinanceDeptId, "FIN", "Finance", "Finance Department"),
            (FixedGuids.MarketingDeptId, "MKT", "Marketing", "Marketing Department")
        };

        foreach (var (id, code, name, description) in deptList)
        {
            departments[name] = id;
            context.Departments.Add(new Department
            {
                Id = id,
                Code = code,
                Name = name,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} departments", departments.Count);
        return departments;
    }

    private static async Task<Dictionary<string, Guid>> SeedSystemRoles(
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        logger.LogInformation("Creating system roles...");
        var roles = new Dictionary<string, Guid>();

        var roleList = new[]
        {
            (FixedGuids.SuperAdminRoleId, "SUPER_ADM", "SuperAdmin", "Super Administrator with global access"),
            (FixedGuids.DepartmentAdminRoleId, "DEPT_ADM", "DepartmentAdmin", "Department Administrator with full access within department"),
            (FixedGuids.PendingUserRoleId, "PENDING", "PendingUser", "Default role for newly registered users")
        };

        foreach (var (id, code, name, description) in roleList)
        {
            var role = new ApplicationRole
            {
                Id = id,
                Code = code,
                Name = name,
                NormalizedName = name.ToUpper(),
                Description = description,
                DepartmentId = null, // System roles have no department
                IsActive = true
            };
            await roleManager.CreateAsync(role);
            roles[name] = id;
        }

        logger.LogInformation("Created {Count} system roles", roles.Count);
        return roles;
    }

    private static async Task<Dictionary<string, Dictionary<string, Guid>>> SeedDepartmentRoles(
        RoleManager<ApplicationRole> roleManager,
        Dictionary<string, Guid> departments,
        ILogger logger)
    {
        logger.LogInformation("Creating department-specific roles...");
        var departmentRoles = new Dictionary<string, Dictionary<string, Guid>>();

        // Finance Roles
        var financeRoles = new[]
        {
            (FixedGuids.FinanceManagerRoleId, "FIN_MGR", "FinanceManager", "Finance Manager - Full permissions on Finance pages"),
            (FixedGuids.FinanceSupervisorRoleId, "FIN_SUP", "FinanceSupervisor", "Finance Supervisor - No delete permission"),
            (FixedGuids.FinanceStaffRoleId, "FIN_STF", "FinanceStaff", "Finance Staff - View and Create only"),
            (FixedGuids.FinanceInternRoleId, "FIN_INT", "FinanceIntern", "Finance Intern - View only")
        };

        departmentRoles["Finance"] = new Dictionary<string, Guid>();
        foreach (var (id, code, name, description) in financeRoles)
        {
            var role = new ApplicationRole
            {
                Id = id,
                Code = code,
                Name = name,
                NormalizedName = name.ToUpper(),
                Description = description,
                DepartmentId = departments["Finance"],
                IsActive = true
            };
            await roleManager.CreateAsync(role);
            departmentRoles["Finance"][name] = id;
        }

        // Marketing Roles
        var marketingRoles = new[]
        {
            (FixedGuids.MarketingManagerRoleId, "MKT_MGR", "MarketingManager", "Marketing Manager - Full permissions on Marketing pages"),
            (FixedGuids.MarketingSupervisorRoleId, "MKT_SUP", "MarketingSupervisor", "Marketing Supervisor - No delete permission"),
            (FixedGuids.MarketingStaffRoleId, "MKT_STF", "MarketingStaff", "Marketing Staff - View and Create only"),
            (FixedGuids.MarketingInternRoleId, "MKT_INT", "MarketingIntern", "Marketing Intern - View only")
        };

        departmentRoles["Marketing"] = new Dictionary<string, Guid>();
        foreach (var (id, code, name, description) in marketingRoles)
        {
            var role = new ApplicationRole
            {
                Id = id,
                Code = code,
                Name = name,
                NormalizedName = name.ToUpper(),
                Description = description,
                DepartmentId = departments["Marketing"],
                IsActive = true
            };
            await roleManager.CreateAsync(role);
            departmentRoles["Marketing"][name] = id;
        }

        logger.LogInformation("Created {Count} department roles", financeRoles.Length + marketingRoles.Length);
        return departmentRoles;
    }

    /// <summary>
    /// Creates Features (Menu structure) with CORRECT hierarchy:
    /// - Dashboard, RBAC Management, Mappings, Account are Main Menus (Level 0) 
    ///   with pages mapped DIRECTLY (no submenus)
    /// - Finance Management is Main Menu with Company SubMenu which has pages
    /// </summary>
    private static async Task<Dictionary<string, Guid>> SeedFeatures(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating features (menu structure)...");
        var features = new Dictionary<string, Guid>();

        // ============================================
        // 1. Dashboard (Main Menu) - Level 0
        //    Pages: /dashboard (mapped directly to this menu)
        // ============================================
        features["Dashboard"] = FixedGuids.DashboardFeatureId;
        context.Features.Add(new Feature
        {
            Id = FixedGuids.DashboardFeatureId,
            Code = "DASH",
            Name = "Dashboard",
            Description = "Dashboard and Home",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 1,
            Icon = "tabler-smart-home",
            IsActive = true,
            RouteUrl = "/dashboard", // Direct route for single-page menu
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ============================================
        // 2. RBAC Management (Main Menu) - Level 0
        //    Pages: /department, /role, /feature, /page, /permission
        //    (pages mapped directly, NO submenus)
        // ============================================
        features["RBAC Management"] = FixedGuids.RbacManagementFeatureId;
        context.Features.Add(new Feature
        {
            Id = FixedGuids.RbacManagementFeatureId,
            Code = "RBAC_MGT",
            Name = "RBAC Management",
            Description = "Role-Based Access Control Management",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 2,
            Icon = "tabler-user-cog",
            IsActive = true,
            RouteUrl = null, // Container menu, no direct route
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ============================================
        // 3. Mappings (Main Menu) - Level 0
        //    Pages: /rolehierarchymapping, /userrolemapping, /rolefeaturemapping, 
        //           /pagefeaturemapping, /rolepagepermissionmapping
        //    (pages mapped directly, NO submenus)
        // ============================================
        features["Mappings"] = FixedGuids.MappingsFeatureId;
        context.Features.Add(new Feature
        {
            Id = FixedGuids.MappingsFeatureId,
            Code = "MAPPINGS",
            Name = "Mappings",
            Description = "Role and Permission Mappings",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 3,
            Icon = "tabler-arrows-left-right",
            IsActive = true,
            RouteUrl = null, // Container menu, no direct route
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ============================================
        // 4. Account (Main Menu) - Level 0
        //    Pages: /profile, /change-password
        //    (pages mapped directly, NO submenus)
        // ============================================
        features["Account"] = FixedGuids.AccountSettingsFeatureId;
        context.Features.Add(new Feature
        {
            Id = FixedGuids.AccountSettingsFeatureId,
            Code = "ACCOUNT",
            Name = "Account",
            Description = "User Account",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 4,
            Icon = "tabler-settings",
            IsActive = true,
            RouteUrl = null, // Container menu, no direct route
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ============================================
        // 5. Finance Management (Main Menu) - Level 0
        //    This one HAS a SubMenu: Company
        // ============================================
        features["Finance Management"] = FixedGuids.FinanceManagementFeatureId;
        context.Features.Add(new Feature
        {
            Id = FixedGuids.FinanceManagementFeatureId,
            Code = "FIN_MGT",
            Name = "Finance Management",
            Description = "Finance Department Operations",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 5,
            Icon = "tabler-currency-rupee",
            IsActive = true,
            RouteUrl = null, // Container menu, no direct route
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Company SubMenu (Level 1) under Finance Management
        // This is the ONLY submenu - pages /company, /testcategories, /testproducts go under here
        features["Company"] = FixedGuids.CompanySubMenuFeatureId;
        context.Features.Add(new Feature
        {
            Id = FixedGuids.CompanySubMenuFeatureId,
            Code = "COMPANY",
            Name = "Company",
            Description = "Company Management SubMenu",
            IsMainMenu = false,
            ParentFeatureId = FixedGuids.FinanceManagementFeatureId,
            DisplayOrder = 1,
            Icon = "tabler-building-skyscraper",
            IsActive = true,
            RouteUrl = null, // Submenu container, no direct route
            Level = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} features", features.Count);
        return features;
    }

    private static async Task<Dictionary<string, Guid>> SeedPages(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating pages...");
        var pages = new Dictionary<string, Guid>();

        // All pages with fixed IDs
        var pageList = new[]
        {
            // Dashboard - mapped to Dashboard menu
            (FixedGuids.DashboardPageId, "DASHBOARD", "Dashboard", "/dashboard", "Dashboard page", "Dashboard", "/api/dashboard", "GET", 1),
            
            // RBAC Management Pages - mapped directly to RBAC Management menu
            (FixedGuids.DepartmentPageId, "DEPT", "Department", "/department", "Department management page", "RBAC Management", "/api/department", "GET", 2),
            (FixedGuids.RolePageId, "ROLE", "Role", "/role", "Role management page", "RBAC Management", "/api/role", "GET", 3),
            (FixedGuids.FeaturePageId, "FEAT", "Feature", "/feature", "Feature management page", "RBAC Management", "/api/feature", "GET", 4),
            (FixedGuids.PagePageId, "PAGE", "Page", "/page", "Page management page", "RBAC Management", "/api/page", "GET", 5),
            (FixedGuids.PermissionPageId, "PERM", "Permission", "/permission", "Permission management page", "RBAC Management", "/api/permission", "GET", 6),
            
            // Mapping Pages - mapped directly to Mappings menu
            (FixedGuids.RoleHierarchyMappingPageId, "ROLE_HIER", "Role Hierarchy", "/rolehierarchymapping", "Role hierarchy management", "Mappings", "/api/rolehierarchymapping", "GET", 7),
            (FixedGuids.UserRoleMappingPageId, "USER_ROLE", "User Role", "/userrolemapping", "User role assignment page", "Mappings", "/api/userrolemapping", "GET", 8),
            (FixedGuids.RoleFeatureMappingPageId, "ROLE_FEAT", "Role Feature", "/rolefeaturemapping", "Role feature mapping page", "Mappings", "/api/rolefeaturemapping", "GET", 9),
            (FixedGuids.PageFeatureMappingPageId, "PAGE_FEAT", "Page Feature", "/pagefeaturemapping", "Page feature mapping page", "Mappings", "/api/pagefeaturemapping", "GET", 10),
            (FixedGuids.RolePagePermissionMappingPageId, "RPP_MAP", "Role Page Permission", "/rolepagepermissionmapping", "Role page permission mapping page", "Mappings", "/api/rolepagepermissionmapping", "GET", 11),
            
            // Account Pages - mapped directly to Account menu
            (FixedGuids.ProfilePageId, "PROFILE", "Profile", "/profile", "User profile page", "Account", "/api/profile", "GET", 12),
            (FixedGuids.ChangePasswordPageId, "CHG_PWD", "Change Password", "/change-password", "Change password page", "Account", "/api/auth/change-password", "POST", 13),
            
            // Finance Management - Company Pages - mapped to Company submenu
            (FixedGuids.CompanyPageId, "COMPANY", "Company", "/company", "Company management page", "Company", "/api/company", "GET", 14),
            (FixedGuids.TestCategoriesPageId, "TEST_CAT", "Test Categories", "/testcategories", "Test categories page", "Company", "/api/testcategories", "GET", 15),
            (FixedGuids.TestProductsPageId, "TEST_PROD", "Test Products", "/testproducts", "Test products page", "Company", "/api/testproducts", "GET", 16)
        };

        foreach (var (id, code, name, url, desc, menuContext, apiEndpoint, httpMethod, order) in pageList)
        {
            pages[name] = id;
            context.Pages.Add(new Page
            {
                Id = id,
                Code = code,
                Name = name,
                Url = url,
                Description = desc,
                MenuContext = menuContext,
                ApiEndpoint = apiEndpoint,
                HttpMethod = httpMethod,
                DisplayOrder = order,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} pages", pages.Count);
        return pages;
    }

    /// <summary>
    /// Creates Page-Feature Mappings with CORRECT hierarchy:
    /// - Dashboard page → Dashboard menu (direct)
    /// - RBAC pages → RBAC Management menu (direct, NO intermediate submenus)
    /// - Mapping pages → Mappings menu (direct, NO intermediate submenus)
    /// - Account pages → Account menu (direct, NO intermediate submenus)
    /// - Company pages → Company submenu (under Finance Management)
    /// </summary>
    private static async Task SeedPageFeatureMappings(
        AppDbContext context,
        Dictionary<string, Guid> features,
        Dictionary<string, Guid> pages,
        ILogger logger)
    {
        logger.LogInformation("Creating page-feature mappings with CORRECT hierarchy...");

        // Page Name -> Feature Name (menu it belongs to)
        var mappings = new[]
        {
            // Dashboard page → Dashboard menu
            ("Dashboard", "Dashboard"),
            
            // RBAC Management pages → RBAC Management menu (DIRECTLY, no submenus)
            ("Department", "RBAC Management"),
            ("Role", "RBAC Management"),
            ("Feature", "RBAC Management"),
            ("Page", "RBAC Management"),
            ("Permission", "RBAC Management"),
            
            // Mappings pages → Mappings menu (DIRECTLY, no submenus)
            ("Role Hierarchy", "Mappings"),
            ("User Role", "Mappings"),
            ("Role Feature", "Mappings"),
            ("Page Feature", "Mappings"),
            ("Role Page Permission", "Mappings"),
            
            // Account pages → Account menu (DIRECTLY, no submenus)
            ("Profile", "Account"),
            ("Change Password", "Account"),
            
            // Company pages → Company submenu (under Finance Management)
            ("Company", "Company"),
            ("Test Categories", "Company"),
            ("Test Products", "Company")
        };

        foreach (var (pageName, featureName) in mappings)
        {
            if (pages.TryGetValue(pageName, out var pageId) && features.TryGetValue(featureName, out var featureId))
            {
                context.PageFeatureMappings.Add(new PageFeatureMapping
                {
                    Id = Guid.NewGuid(),
                    PageId = pageId,
                    FeatureId = featureId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                logger.LogWarning("Could not create page-feature mapping: Page '{Page}' or Feature '{Feature}' not found", pageName, featureName);
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created page-feature mappings");
    }

    private static async Task SeedRoleFeatureMappings(
        AppDbContext context,
        Dictionary<string, Guid> systemRoles,
        Dictionary<string, Dictionary<string, Guid>> departmentRoles,
        Dictionary<string, Guid> features,
        Dictionary<string, Guid> departments,
        ILogger logger)
    {
        logger.LogInformation("Creating role-feature mappings...");

        // SuperAdmin gets ALL features (no department restriction)
        foreach (var feature in features)
        {
            context.RoleFeatureMappings.Add(new RoleFeatureMapping
            {
                Id = Guid.NewGuid(),
                RoleId = systemRoles["SuperAdmin"],
                FeatureId = feature.Value,
                DepartmentId = null, // No department restriction for SuperAdmin
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // DepartmentAdmin gets ALL features but scoped to their department
        foreach (var deptName in new[] { "Finance", "Marketing" })
        {
            foreach (var feature in features)
            {
                context.RoleFeatureMappings.Add(new RoleFeatureMapping
                {
                    Id = Guid.NewGuid(),
                    RoleId = systemRoles["DepartmentAdmin"],
                    FeatureId = feature.Value,
                    DepartmentId = departments[deptName],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Department roles get only specific features
        // They can access: Dashboard, Account, Finance Management (with Company submenu)
        var deptRoleFeatures = new[] { "Dashboard", "Account", "Finance Management", "Company" };
        
        foreach (var deptName in new[] { "Finance", "Marketing" })
        {
            foreach (var roleName in departmentRoles[deptName].Keys)
            {
                foreach (var featureName in deptRoleFeatures)
                {
                    if (features.TryGetValue(featureName, out var featureId))
                    {
                        context.RoleFeatureMappings.Add(new RoleFeatureMapping
                        {
                            Id = Guid.NewGuid(),
                            RoleId = departmentRoles[deptName][roleName],
                            FeatureId = featureId,
                            DepartmentId = departments[deptName],
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created role-feature mappings");
    }

    private static async Task SeedRolePagePermissionMappings(
        AppDbContext context,
        Dictionary<string, Guid> systemRoles,
        Dictionary<string, Dictionary<string, Guid>> departmentRoles,
        Dictionary<string, Guid> pages,
        Dictionary<string, Guid> permissions,
        Dictionary<string, Guid> departments,
        ILogger logger)
    {
        logger.LogInformation("Creating role-page-permission mappings...");

        // SuperAdmin gets ALL permissions on ALL pages (no department restriction)
        foreach (var page in pages)
        {
            foreach (var permission in permissions)
            {
                context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                {
                    Id = Guid.NewGuid(),
                    RoleId = systemRoles["SuperAdmin"],
                    PageId = page.Value,
                    PermissionId = permission.Value,
                    DepartmentId = null, // No department restriction
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // DepartmentAdmin gets all permissions on all pages EXCEPT Department page (View only)
        foreach (var deptName in new[] { "Finance", "Marketing" })
        {
            foreach (var page in pages)
            {
                if (page.Key == "Department")
                {
                    // Department page - View only for DepartmentAdmin
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = systemRoles["DepartmentAdmin"],
                        PageId = page.Value,
                        PermissionId = permissions["View"],
                        DepartmentId = departments[deptName],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    // All other pages - full permissions
                    foreach (var permission in permissions)
                    {
                        context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                        {
                            Id = Guid.NewGuid(),
                            RoleId = systemRoles["DepartmentAdmin"],
                            PageId = page.Value,
                            PermissionId = permission.Value,
                            DepartmentId = departments[deptName],
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        // Department roles only have access to 5 pages: Profile, Change Password, Company, Test Categories, Test Products
        var deptRolePages = new[] { "Profile", "Change Password", "Company", "Test Categories", "Test Products" };

        // Permission matrix:
        // Manager: Create, View, Update, Delete (all)
        // Supervisor: Create, View, Update (no Delete)
        // Staff: View, Create
        // Intern: View only

        foreach (var deptName in new[] { "Finance", "Marketing" })
        {
            var rolePrefix = deptName;
            
            foreach (var pageName in deptRolePages)
            {
                if (!pages.TryGetValue(pageName, out var pageId)) continue;

                // Manager - all permissions
                foreach (var perm in permissions)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles[deptName][$"{rolePrefix}Manager"],
                        PageId = pageId,
                        PermissionId = perm.Value,
                        DepartmentId = departments[deptName],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Supervisor - no Delete
                var supervisorPerms = new[] { "Create", "View", "Update" };
                foreach (var permName in supervisorPerms)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles[deptName][$"{rolePrefix}Supervisor"],
                        PageId = pageId,
                        PermissionId = permissions[permName],
                        DepartmentId = departments[deptName],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Staff - View and Create only
                var staffPerms = new[] { "View", "Create" };
                foreach (var permName in staffPerms)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles[deptName][$"{rolePrefix}Staff"],
                        PageId = pageId,
                        PermissionId = permissions[permName],
                        DepartmentId = departments[deptName],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Intern - View only
                context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                {
                    Id = Guid.NewGuid(),
                    RoleId = departmentRoles[deptName][$"{rolePrefix}Intern"],
                    PageId = pageId,
                    PermissionId = permissions["View"],
                    DepartmentId = departments[deptName],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created role-page-permission mappings");
    }

    private static async Task SeedRoleHierarchies(
        AppDbContext context,
        Dictionary<string, Dictionary<string, Guid>> departmentRoles,
        Dictionary<string, Guid> departments,
        ILogger logger)
    {
        logger.LogInformation("Creating role hierarchies...");

        // Finance Department hierarchy: Manager -> Supervisor -> Staff -> Intern
        var financeHierarchy = new[]
        {
            ("FinanceManager", "FinanceSupervisor", 0),
            ("FinanceSupervisor", "FinanceStaff", 1),
            ("FinanceStaff", "FinanceIntern", 2)
        };

        foreach (var (parentRole, childRole, level) in financeHierarchy)
        {
            context.RoleHierarchies.Add(new RoleHierarchy
            {
                Id = Guid.NewGuid(),
                ParentRoleId = departmentRoles["Finance"][parentRole],
                ChildRoleId = departmentRoles["Finance"][childRole],
                DepartmentId = departments["Finance"],
                Level = level,  // FIXED: Changed from HierarchyLevel to Level
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Marketing Department hierarchy: Manager -> Supervisor -> Staff -> Intern
        var marketingHierarchy = new[]
        {
            ("MarketingManager", "MarketingSupervisor", 0),
            ("MarketingSupervisor", "MarketingStaff", 1),
            ("MarketingStaff", "MarketingIntern", 2)
        };

        foreach (var (parentRole, childRole, level) in marketingHierarchy)
        {
            context.RoleHierarchies.Add(new RoleHierarchy
            {
                Id = Guid.NewGuid(),
                ParentRoleId = departmentRoles["Marketing"][parentRole],
                ChildRoleId = departmentRoles["Marketing"][childRole],
                DepartmentId = departments["Marketing"],
                Level = level,  // FIXED: Changed from HierarchyLevel to Level
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created role hierarchies");
    }

    private static async Task SeedCountryTimeZones(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Checking for CountryTimeZone seed data...");

        // Check if Countries and TimeZones exist
        var countriesExist = await context.Set<Country>().AnyAsync();
        var timeZonesExist = await context.Set<TimeZoneMaster>().AnyAsync();
        
        if (!countriesExist || !timeZonesExist)
        {
            logger.LogInformation("Countries or TimeZones not found, skipping CountryTimeZone seeding");
            return;
        }

        // Check if mappings already exist
        var mappingsExist = await context.Set<CountryTimeZone>().AnyAsync();
        if (mappingsExist)
        {
            logger.LogInformation("CountryTimeZone mappings already exist, skipping");
            return;
        }

        // Get sample countries and timezones for mapping
        var countries = await context.Set<Country>().Take(5).ToListAsync();
        var timeZones = await context.Set<TimeZoneMaster>().Take(10).ToListAsync();

        if (countries.Count == 0 || timeZones.Count == 0)
        {
            logger.LogInformation("No countries or timezones to map");
            return;
        }

        // Create sample mappings - each country gets 2-3 timezones
        foreach (var country in countries)
        {
            var countryTimeZones = timeZones.Take(Math.Min(3, timeZones.Count)).ToList();
            foreach (var tz in countryTimeZones)
            {
                context.Set<CountryTimeZone>().Add(new CountryTimeZone
                {
                    Id = Guid.NewGuid(),
                    CountryId = country.Id,
                    TimeZoneId = tz.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created CountryTimeZone mappings");
    }

    private static async Task SeedUsers(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        Dictionary<string, Guid> systemRoles,
        Dictionary<string, Dictionary<string, Guid>> departmentRoles,
        Dictionary<string, Guid> departments,
        ILogger logger)
    {
        logger.LogInformation("Creating 20 test users...");

        // User definitions: (Id, Email, FirstName, LastName, RoleName, DepartmentId, RoleId, Password)
        // NOTE: ApplicationUser does NOT have DepartmentId - department association is through UserRoleMapping
        var users = new[]
        {
            // SuperAdmin users (2)
            (FixedGuids.SuperAdminUserId, "superadmin@company.com", "Super", "Admin", "SuperAdmin", (Guid?)null, systemRoles["SuperAdmin"], "SuperAdmin@123"),
            (FixedGuids.SuperAdmin2UserId, "superadmin2@company.com", "Super", "Admin2", "SuperAdmin", (Guid?)null, systemRoles["SuperAdmin"], "SuperAdmin@123"),
            
            // Finance Department users (9)
            (FixedGuids.FinanceAdminUserId, "finance.admin@company.com", "Finance", "Admin", "DepartmentAdmin", (Guid?)departments["Finance"], systemRoles["DepartmentAdmin"], "Finance@123"),
            (FixedGuids.FinanceManagerUserId, "finance.manager@company.com", "Finance", "Manager", "FinanceManager", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceManager"], "Finance@123"),
            (FixedGuids.FinanceManager2UserId, "finance.manager2@company.com", "Finance", "Manager2", "FinanceManager", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceManager"], "Finance@123"),
            (FixedGuids.FinanceSupervisorUserId, "finance.supervisor@company.com", "Finance", "Supervisor", "FinanceSupervisor", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceSupervisor"], "Finance@123"),
            (FixedGuids.FinanceSupervisor2UserId, "finance.supervisor2@company.com", "Finance", "Supervisor2", "FinanceSupervisor", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceSupervisor"], "Finance@123"),
            (FixedGuids.FinanceStaffUserId, "finance.staff@company.com", "Finance", "Staff", "FinanceStaff", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceStaff"], "Finance@123"),
            (FixedGuids.FinanceStaff2UserId, "finance.staff2@company.com", "Finance", "Staff2", "FinanceStaff", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceStaff"], "Finance@123"),
            (FixedGuids.FinanceInternUserId, "finance.intern@company.com", "Finance", "Intern", "FinanceIntern", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceIntern"], "Finance@123"),
            (FixedGuids.FinanceIntern2UserId, "finance.intern2@company.com", "Finance", "Intern2", "FinanceIntern", (Guid?)departments["Finance"], departmentRoles["Finance"]["FinanceIntern"], "Finance@123"),
            
            // Marketing Department users (9)
            (FixedGuids.MarketingAdminUserId, "marketing.admin@company.com", "Marketing", "Admin", "DepartmentAdmin", (Guid?)departments["Marketing"], systemRoles["DepartmentAdmin"], "Marketing@123"),
            (FixedGuids.MarketingManagerUserId, "marketing.manager@company.com", "Marketing", "Manager", "MarketingManager", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingManager"], "Marketing@123"),
            (FixedGuids.MarketingManager2UserId, "marketing.manager2@company.com", "Marketing", "Manager2", "MarketingManager", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingManager"], "Marketing@123"),
            (FixedGuids.MarketingSupervisorUserId, "marketing.supervisor@company.com", "Marketing", "Supervisor", "MarketingSupervisor", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingSupervisor"], "Marketing@123"),
            (FixedGuids.MarketingSupervisor2UserId, "marketing.supervisor2@company.com", "Marketing", "Supervisor2", "MarketingSupervisor", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingSupervisor"], "Marketing@123"),
            (FixedGuids.MarketingStaffUserId, "marketing.staff@company.com", "Marketing", "Staff", "MarketingStaff", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingStaff"], "Marketing@123"),
            (FixedGuids.MarketingStaff2UserId, "marketing.staff2@company.com", "Marketing", "Staff2", "MarketingStaff", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingStaff"], "Marketing@123"),
            (FixedGuids.MarketingInternUserId, "marketing.intern@company.com", "Marketing", "Intern", "MarketingIntern", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingIntern"], "Marketing@123"),
            (FixedGuids.MarketingIntern2UserId, "marketing.intern2@company.com", "Marketing", "Intern2", "MarketingIntern", (Guid?)departments["Marketing"], departmentRoles["Marketing"]["MarketingIntern"], "Marketing@123")
        };

        foreach (var (id, email, firstName, lastName, roleName, departmentId, roleId, password) in users)
        {
            // FIXED: ApplicationUser does NOT have DepartmentId property
            // Department association is through UserRoleMapping only
            var user = new ApplicationUser
            {
                Id = id,
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Add to role using UserManager
                await userManager.AddToRoleAsync(user, roleName);
                
                // Create UserRoleMapping entry (this is where DepartmentId is stored)
                context.UserRoleMappings.Add(new UserRoleMapping
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    RoleId = roleId,
                    DepartmentId = departmentId,  // Department association is here
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                
                logger.LogInformation("Created user: {Email} with role: {Role}", email, roleName);
            }
            else
            {
                logger.LogError("Failed to create user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} users", users.Length);
    }
}
