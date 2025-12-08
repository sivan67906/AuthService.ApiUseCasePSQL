using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence;

/// <summary>
/// Comprehensive seed data - REMOVED: RolePermissionMapping, PagePermissionMapping, RoleDepartmentMapping, Addresses
/// Creates:
/// - Departments (Finance, Marketing)
/// - System Roles (SuperAdmin, DepartmentAdmin, PendingUser)
/// - Department Roles (FinanceManager, FinanceSupervisor, FinanceStaff, FinanceIntern, etc.)
/// - 6 Test Users with proper UserRoleMappings
/// - Complete RBAC structure with Features (menus), Pages, RoleFeatureMapping, RolePagePermissionMapping
/// - RoleHierarchy for departments
/// - Permissions (Create, View, Update, Delete)
/// 
/// REMOVED ITEMS (4 total):
/// 1. RolePermissionMapping (seeding - table was never populated)
/// 2. PagePermissionMapping (seeding - table will not be populated)
/// 3. "Role Permission Mapping" menu and page
/// 4. "Page Permission Mapping" menu and page
/// 5. "Role Department Mapping" menu and page (redundant - ApplicationRole.DepartmentId already provides this)
/// 6. "Addresses" menu and page from Account Settings
/// 
/// TOTAL FEATURES: 26 (originally 30, removed 4: RolePermissionMapping, PagePermissionMapping, RoleDepartmentMapping, Addresses)
/// TOTAL PAGES: 17 (originally 21, removed 4: RolePermissionMapping, PagePermissionMapping, RoleDepartmentMapping, Addresses)
/// MAPPINGS SUBMENU: 5 items (originally 8, removed 3: RolePermissionMapping, PagePermissionMapping, RoleDepartmentMapping)
/// 
/// NOTE: ApplicationRole already has DepartmentId field, making RoleDepartmentMapping table redundant.
///       The many-to-many relationship is not used in this architecture.
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

        // Test Users
        public static readonly Guid SuperAdminUserId = Guid.Parse("99999999-9999-9999-9999-000000000001");
        public static readonly Guid FinanceAdminUserId = Guid.Parse("99999999-9999-9999-9999-000000000002");
        public static readonly Guid FinanceManagerUserId = Guid.Parse("99999999-9999-9999-9999-000000000003");
        public static readonly Guid FinanceSupervisorUserId = Guid.Parse("99999999-9999-9999-9999-000000000004");
        public static readonly Guid FinanceStaffUserId = Guid.Parse("99999999-9999-9999-9999-000000000005");
        public static readonly Guid FinanceInternUserId = Guid.Parse("99999999-9999-9999-9999-000000000006");
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

            // 5. Create Features (Menu structure)
            var features = await SeedFeatures(context, logger);

            // 6. Create Pages
            var pages = await SeedPages(context, logger);

            // 7. Create Page-Feature Mappings
            await SeedPageFeatureMappings(context, features, pages, logger);

            // 8. Create Role-Feature Mappings (with department scope)
            await SeedRoleFeatureMappings(context, systemRoles, departmentRoles, features, departments, logger);

            // 9. Create Role-Page-Permission Mappings (with department scope)
            await SeedRolePagePermissionMappings(context, systemRoles, departmentRoles, pages, permissions, departments, logger);

            // 10. Create Role Hierarchies
            await SeedRoleHierarchies(context, departmentRoles, departments, logger);

            // 11. Create Test Users
            await SeedTestUsers(userManager, context, systemRoles, departmentRoles, departments, logger);

            await context.SaveChangesAsync();
            logger.LogInformation("Comprehensive database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding");
            throw;
        }
    }

    private static async Task<Dictionary<string, Guid>> SeedPermissions(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating permissions...");
        var permissions = new Dictionary<string, Guid>();

        var permissionList = new[]
        {
            ("Create", "Permission to create records"),
            ("View", "Permission to view records"),
            ("Update", "Permission to update records"),
            ("Delete", "Permission to delete records")
        };

        foreach (var (name, description) in permissionList)
        {
            var permId = Guid.NewGuid();
            permissions[name] = permId;
            context.Permissions.Add(new Permission
            {
                Id = permId,
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
        var departments = new Dictionary<string, Guid>
        {
            ["Finance"] = FixedGuids.FinanceDeptId,
            ["Marketing"] = FixedGuids.MarketingDeptId
        };

        context.Departments.AddRange(
            new Department
            {
                Id = FixedGuids.FinanceDeptId,
                Name = "Finance",
                Description = "Finance Department - Manages financial operations, budgeting, accounting, and financial reporting",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = FixedGuids.MarketingDeptId,
                Name = "Marketing",
                Description = "Marketing Department - Manages marketing campaigns, branding, and customer engagement",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} departments", departments.Count);
        return departments;
    }

    private static async Task<Dictionary<string, Guid>> SeedSystemRoles(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        logger.LogInformation("Creating system roles...");
        var roles = new Dictionary<string, Guid>();

        var systemRolesList = new[]
        {
            (FixedGuids.SuperAdminRoleId, SystemRoles.SuperAdmin, "Global administrator with unrestricted access"),
            (FixedGuids.DepartmentAdminRoleId, SystemRoles.DepartmentAdmin, "Department administrator with full access within their department"),
            (FixedGuids.PendingUserRoleId, SystemRoles.PendingUser, "Newly registered user awaiting role assignment")
        };

        foreach (var (id, name, description) in systemRolesList)
        {
            var role = new ApplicationRole
            {
                Id = id,
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
            (FixedGuids.FinanceManagerRoleId, "FinanceManager", "Finance Manager - Full permissions on Finance pages"),
            (FixedGuids.FinanceSupervisorRoleId, "FinanceSupervisor", "Finance Supervisor - No delete permission"),
            (FixedGuids.FinanceStaffRoleId, "FinanceStaff", "Finance Staff - View and Create only"),
            (FixedGuids.FinanceInternRoleId, "FinanceIntern", "Finance Intern - View only")
        };

        departmentRoles["Finance"] = new Dictionary<string, Guid>();
        foreach (var (id, name, description) in financeRoles)
        {
            var role = new ApplicationRole
            {
                Id = id,
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
            (FixedGuids.MarketingManagerRoleId, "MarketingManager", "Marketing Manager - Full permissions on Marketing pages"),
            (FixedGuids.MarketingSupervisorRoleId, "MarketingSupervisor", "Marketing Supervisor - No delete permission"),
            (FixedGuids.MarketingStaffRoleId, "MarketingStaff", "Marketing Staff - View and Create only"),
            (FixedGuids.MarketingInternRoleId, "MarketingIntern", "Marketing Intern - View only")
        };

        departmentRoles["Marketing"] = new Dictionary<string, Guid>();
        foreach (var (id, name, description) in marketingRoles)
        {
            var role = new ApplicationRole
            {
                Id = id,
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

    private static async Task<Dictionary<string, Guid>> SeedFeatures(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating features (menu structure)...");
        var features = new Dictionary<string, Guid>();

        // ============================================
        // 1. Dashboard (Main Menu)
        // ============================================
        var dashboardId = Guid.NewGuid();
        features["Dashboard"] = dashboardId;
        context.Features.Add(new Feature
        {
            Id = dashboardId,
            Name = "Dashboard",
            Description = "Dashboard and Home",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 1,
            Icon = "ri-dashboard-line",
            IsActive = true,
            RouteUrl = "/",
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // ============================================
        // 2. RBAC Management (Main Menu)
        // ============================================
        var rbacId = Guid.NewGuid();
        features["RBAC Management"] = rbacId;
        context.Features.Add(new Feature
        {
            Id = rbacId,
            Name = "RBAC Management",
            Description = "Role-Based Access Control Management",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 2,
            Icon = "ri-shield-user-line",
            IsActive = true,
            RouteUrl = null,
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // RBAC Submenus
        var rbacSubmenus = new[]
        {
            ("Departments", "Department Management", 1, "/department", "ri-building-line"),
            ("Roles", "Role Management", 2, "/role", "ri-user-settings-line"),
            ("Features", "Feature Management", 3, "/feature", "ri-function-line"),
            ("Pages", "Page Management", 4, "/page", "ri-file-list-line"),
            ("Permissions", "Permission Management", 5, "/permission", "ri-key-2-line")
        };

        foreach (var (name, desc, order, route, icon) in rbacSubmenus)
        {
            var featureId = Guid.NewGuid();
            features[name] = featureId;
            context.Features.Add(new Feature
            {
                Id = featureId,
                Name = name,
                Description = desc,
                IsMainMenu = false,
                ParentFeatureId = rbacId,
                DisplayOrder = order,
                Icon = icon,
                IsActive = true,
                RouteUrl = route,
                Level = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        // ============================================
        // 3. Mappings (Main Menu)
        // ============================================
        var mappingsId = Guid.NewGuid();
        features["Mappings"] = mappingsId;
        context.Features.Add(new Feature
        {
            Id = mappingsId,
            Name = "Mappings",
            Description = "Role and Permission Mappings",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 3,
            Icon = "ri-links-line",
            IsActive = true,
            RouteUrl = null,
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Mappings Submenus
        var mappingsSubmenus = new[]
        {
            ("User Role Mapping", "Assign roles to users", 1, "/userrolemapping", "ri-user-add-line"),
            ("Role Hierarchy", "Manage role hierarchy", 2, "/rolehierarchymapping", "ri-organization-chart"),
            ("Role Feature Mapping", "Map roles to features", 3, "/rolefeaturemapping", "ri-menu-add-line"),
            ("Role Page Permission Mapping", "Map roles to page permissions", 4, "/rolepagepermissionmapping", "ri-file-shield-line"),
            ("Page Feature Mapping", "Map pages to features", 5, "/pagefeaturemapping", "ri-pages-line")
        };

        foreach (var (name, desc, order, route, icon) in mappingsSubmenus)
        {
            var featureId = Guid.NewGuid();
            features[name] = featureId;
            context.Features.Add(new Feature
            {
                Id = featureId,
                Name = name,
                Description = desc,
                IsMainMenu = false,
                ParentFeatureId = mappingsId,
                DisplayOrder = order,
                Icon = icon,
                IsActive = true,
                RouteUrl = route,
                Level = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        // ============================================
        // 4. Account Settings (Main Menu)
        // ============================================
        var accountSettingsId = Guid.NewGuid();
        features["Account Settings"] = accountSettingsId;
        context.Features.Add(new Feature
        {
            Id = accountSettingsId,
            Name = "Account Settings",
            Description = "User Account Settings",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 4,
            Icon = "ri-user-settings-line",
            IsActive = true,
            RouteUrl = null,
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Account Settings Submenus
        var accountSubmenus = new[]
        {
            ("Profile", "View and edit profile", 1, "/profile", "ri-user-line"),
            ("Change Password", "Change account password", 2, "/change-password", "ri-lock-password-line"),
            ("Two-Factor Authentication", "Two-factor authentication settings", 3, "/two-factor", "ri-shield-check-line"),
            ("Authenticator Setup", "Setup authenticator app", 4, "/authenticator-setup", "ri-qr-code-line")
        };

        foreach (var (name, desc, order, route, icon) in accountSubmenus)
        {
            var featureId = Guid.NewGuid();
            features[name] = featureId;
            context.Features.Add(new Feature
            {
                Id = featureId,
                Name = name,
                Description = desc,
                IsMainMenu = false,
                ParentFeatureId = accountSettingsId,
                DisplayOrder = order,
                Icon = icon,
                IsActive = true,
                RouteUrl = route,
                Level = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        // ============================================
        // 5. Finance Management (Main Menu)
        // ============================================
        var financeId = Guid.NewGuid();
        features["Finance Management"] = financeId;
        context.Features.Add(new Feature
        {
            Id = financeId,
            Name = "Finance Management",
            Description = "Finance Department Operations",
            IsMainMenu = true,
            ParentFeatureId = null,
            DisplayOrder = 5,
            Icon = "ri-money-dollar-circle-line",
            IsActive = true,
            RouteUrl = null,
            Level = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Finance Submenus
        var financeSubmenus = new[]
        {
            ("Test Categories", "Test Categories Management", 1, "/testcategories", "ri-folder-line"),
            ("Test Products", "Test Products Management", 2, "/testproducts", "ri-shopping-bag-line")
        };

        foreach (var (name, desc, order, route, icon) in financeSubmenus)
        {
            var featureId = Guid.NewGuid();
            features[name] = featureId;
            context.Features.Add(new Feature
            {
                Id = featureId,
                Name = name,
                Description = desc,
                IsMainMenu = false,
                ParentFeatureId = financeId,
                DisplayOrder = order,
                Icon = icon,
                IsActive = true,
                RouteUrl = route,
                Level = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created {Count} features", features.Count);
        return features;
    }

    private static string GetIconForFeature(string featureName)
    {
        return featureName switch
        {
            // RBAC Management
            "Departments" => "ri-building-line",
            "Roles" => "ri-user-settings-line",
            "Features" => "ri-function-line",
            "Pages" => "ri-file-list-line",
            "Permissions" => "ri-key-2-line",
            // Mappings
            "User Role Mapping" => "ri-user-add-line",
            "Role Hierarchy" => "ri-organization-chart",
            "Role Feature Mapping" => "ri-menu-add-line",
            "Role Page Permission Mapping" => "ri-file-shield-line",
            "Page Feature Mapping" => "ri-pages-line",
            // Account Settings
            "Profile" => "ri-user-line",
            "Change Password" => "ri-lock-password-line",
            "Two-Factor Authentication" => "ri-shield-check-line",
            "Authenticator Setup" => "ri-qr-code-line",
            // Finance
            "Test Categories" => "ri-folder-line",
            "Test Products" => "ri-shopping-bag-line",
            _ => "ri-circle-line"
        };
    }

    private static async Task<Dictionary<string, Guid>> SeedPages(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Creating pages...");
        var pages = new Dictionary<string, Guid>();

        var pageList = new[]
        {
            // Dashboard
            ("Dashboard", "/", "Dashboard page", "Dashboard", "/api/dashboard", "GET", 1),
            
            // RBAC Management Pages
            ("Department List", "/department", "Department management page", "RBAC Management", "/api/department", "GET", 2),
            ("Role List", "/role", "Role management page", "RBAC Management", "/api/role", "GET", 3),
            ("Feature List", "/feature", "Feature management page", "RBAC Management", "/api/feature", "GET", 4),
            ("Page List", "/page", "Page management page", "RBAC Management", "/api/page", "GET", 5),
            ("Permission List", "/permission", "Permission management page", "RBAC Management", "/api/permission", "GET", 6),
            
            // Mapping Pages
            ("User Role Mapping", "/userrolemapping", "User role assignment page", "Mappings", "/api/userrolemapping", "GET", 7),
            ("Role Hierarchy Mapping", "/rolehierarchymapping", "Role hierarchy management", "Mappings", "/api/rolehierarchymapping", "GET", 8),
            ("Role Feature Mapping", "/rolefeaturemapping", "Role feature mapping page", "Mappings", "/api/rolefeaturemapping", "GET", 9),
            ("Role Page Permission Mapping", "/rolepagepermissionmapping", "Role page permission mapping page", "Mappings", "/api/rolepagepermissionmapping", "GET", 10),
            ("Page Feature Mapping", "/pagefeaturemapping", "Page feature mapping page", "Mappings", "/api/pagefeaturemapping", "GET", 11),
            
            // Account Settings Pages
            ("Profile", "/profile", "User profile page", "Account Settings", "/api/profile", "GET", 12),
            ("Change Password", "/change-password", "Change password page", "Account Settings", "/api/auth/change-password", "POST", 13),
            ("Two-Factor Authentication", "/two-factor", "Two-factor authentication settings", "Account Settings", "/api/auth/twofactor", "GET", 14),
            ("Authenticator Setup", "/authenticator-setup", "Authenticator app setup", "Account Settings", "/api/auth/authenticator", "GET", 15),
            
            // Finance Management Pages
            ("Test Categories", "/testcategories", "Finance test categories page", "Finance Management", "/api/testcategories", "GET", 16),
            ("Test Products", "/testproducts", "Finance test products page", "Finance Management", "/api/testproducts", "GET", 17)
        };

        foreach (var (name, url, desc, menuContext, apiEndpoint, httpMethod, order) in pageList)
        {
            var pageId = Guid.NewGuid();
            pages[name] = pageId;
            context.Pages.Add(new Page
            {
                Id = pageId,
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

    private static async Task SeedPageFeatureMappings(
        AppDbContext context,
        Dictionary<string, Guid> features,
        Dictionary<string, Guid> pages,
        ILogger logger)
    {
        logger.LogInformation("Creating page-feature mappings...");

        var mappings = new[]
        {
            // Dashboard
            ("Dashboard", "Dashboard"),
            
            // RBAC Management
            ("Department List", "Departments"),
            ("Role List", "Roles"),
            ("Feature List", "Features"),
            ("Page List", "Pages"),
            ("Permission List", "Permissions"),
            
            // Mappings
            ("User Role Mapping", "User Role Mapping"),
            ("Role Hierarchy Mapping", "Role Hierarchy"),
            ("Role Feature Mapping", "Role Feature Mapping"),
            ("Role Page Permission Mapping", "Role Page Permission Mapping"),
            ("Page Feature Mapping", "Page Feature Mapping"),
            
            // Account Settings
            ("Profile", "Profile"),
            ("Change Password", "Change Password"),
            ("Two-Factor Authentication", "Two-Factor Authentication"),
            ("Authenticator Setup", "Authenticator Setup"),
            
            // Finance
            ("Test Categories", "Test Categories"),
            ("Test Products", "Test Products")
        };

        foreach (var (pageName, featureName) in mappings)
        {
            if (pages.ContainsKey(pageName) && features.ContainsKey(featureName))
            {
                context.PageFeatureMappings.Add(new PageFeatureMapping
                {
                    Id = Guid.NewGuid(),
                    PageId = pages[pageName],
                    FeatureId = features[featureName],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
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

        // SuperAdmin gets ALL features with NULL department
        foreach (var (featureName, featureId) in features)
        {
            context.RoleFeatureMappings.Add(new RoleFeatureMapping
            {
                Id = Guid.NewGuid(),
                RoleId = systemRoles[SystemRoles.SuperAdmin],
                FeatureId = featureId,
                DepartmentId = null, // SuperAdmin has no department restriction
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // CRITICAL FIX: DepartmentAdmin gets ALL features but scoped to their department
        // For Finance Department Admin
        foreach (var (featureName, featureId) in features)
        {
            context.RoleFeatureMappings.Add(new RoleFeatureMapping
            {
                Id = Guid.NewGuid(),
                RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                FeatureId = featureId,
                DepartmentId = departments["Finance"], // Scoped to Finance department
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // For Marketing Department Admin
        foreach (var (featureName, featureId) in features)
        {
            context.RoleFeatureMappings.Add(new RoleFeatureMapping
            {
                Id = Guid.NewGuid(),
                RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                FeatureId = featureId,
                DepartmentId = departments["Marketing"], // Scoped to Marketing department
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Finance roles get Dashboard + RBAC Management + Finance Management features
        var financeFeatures = new[] { "Dashboard", "RBAC Management", "Departments", "Roles", "Features", "Pages",
            "Permissions", "Role Hierarchy", "User Role Assignment", "Finance Management", "Test Categories", "Test Products" };

        foreach (var roleName in new[] { "FinanceManager", "FinanceSupervisor", "FinanceStaff", "FinanceIntern" })
        {
            foreach (var featureName in financeFeatures)
            {
                if (features.ContainsKey(featureName) && departmentRoles["Finance"].ContainsKey(roleName))
                {
                    context.RoleFeatureMappings.Add(new RoleFeatureMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles["Finance"][roleName],
                        FeatureId = features[featureName],
                        DepartmentId = departments["Finance"], // Scoped to Finance department
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Marketing roles get Dashboard + RBAC Management + Marketing Management features
        // (Add Marketing features when you create them)
        var marketingFeatures = new[] { "Dashboard", "RBAC Management", "Departments", "Roles", "Features", "Pages",
            "Permissions", "Role Hierarchy", "User Role Assignment" };

        foreach (var roleName in new[] { "MarketingManager", "MarketingSupervisor", "MarketingStaff", "MarketingIntern" })
        {
            foreach (var featureName in marketingFeatures)
            {
                if (features.ContainsKey(featureName) && departmentRoles["Marketing"].ContainsKey(roleName))
                {
                    context.RoleFeatureMappings.Add(new RoleFeatureMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles["Marketing"][roleName],
                        FeatureId = features[featureName],
                        DepartmentId = departments["Marketing"], // Scoped to Marketing department
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
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

        // SuperAdmin gets ALL permissions on ALL pages with NULL department
        foreach (var (pageName, pageId) in pages)
        {
            foreach (var (permName, permId) in permissions)
            {
                context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                {
                    Id = Guid.NewGuid(),
                    RoleId = systemRoles[SystemRoles.SuperAdmin],
                    PageId = pageId,
                    PermissionId = permId,
                    DepartmentId = null, // SuperAdmin has no department restriction
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // CRITICAL FIX: DepartmentAdmin permissions structure
        // 1. Department List: VIEW ONLY (can see their department but not edit/delete/create)
        // 2. Other RBAC pages: FULL PERMISSIONS (can manage roles, features, pages, etc. within their department)

        var departmentPageName = "Department List";
        var otherRbacPages = new[] { "Dashboard", "Role List", "Feature List", "Page List",
            "Permission List", "Role Hierarchy", "User Role Assignment" };
        var financePages = new[] { "Test Categories", "Test Products" };

        // Finance DepartmentAdmin: VIEW ONLY on Department List
        if (pages.ContainsKey(departmentPageName))
        {
            var pageId = pages[departmentPageName];
            if (permissions.ContainsKey("View"))
            {
                context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                {
                    Id = Guid.NewGuid(),
                    RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                    PageId = pageId,
                    PermissionId = permissions["View"],  // ← ONLY View permission!
                    DepartmentId = departments["Finance"],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Finance DepartmentAdmin: FULL PERMISSIONS on other RBAC pages
        foreach (var pageName in otherRbacPages)
        {
            if (pages.ContainsKey(pageName))
            {
                var pageId = pages[pageName];
                foreach (var (permName, permId) in permissions)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                        PageId = pageId,
                        PermissionId = permId,
                        DepartmentId = departments["Finance"],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Finance DepartmentAdmin: Full permissions on Finance pages
        foreach (var pageName in financePages)
        {
            if (pages.ContainsKey(pageName))
            {
                var pageId = pages[pageName];
                foreach (var (permName, permId) in permissions)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                        PageId = pageId,
                        PermissionId = permId,
                        DepartmentId = departments["Finance"], // Scoped to Finance
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Marketing DepartmentAdmin: VIEW ONLY on Department List
        if (pages.ContainsKey(departmentPageName))
        {
            var pageId = pages[departmentPageName];
            if (permissions.ContainsKey("View"))
            {
                context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                {
                    Id = Guid.NewGuid(),
                    RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                    PageId = pageId,
                    PermissionId = permissions["View"],  // ← ONLY View permission!
                    DepartmentId = departments["Marketing"],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Marketing DepartmentAdmin: FULL PERMISSIONS on other RBAC pages
        foreach (var pageName in otherRbacPages)
        {
            if (pages.ContainsKey(pageName))
            {
                var pageId = pages[pageName];
                foreach (var (permName, permId) in permissions)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = systemRoles[SystemRoles.DepartmentAdmin],
                        PageId = pageId,
                        PermissionId = permId,
                        DepartmentId = departments["Marketing"],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Finance roles - specific permissions per role
        // FinanceManager: Full permissions (Create, View, Update, Delete)
        // Reusing financePages array from above
        foreach (var pageName in financePages)
        {
            if (pages.ContainsKey(pageName))
            {
                var pageId = pages[pageName];

                // FinanceManager - All permissions
                foreach (var (permName, permId) in permissions)
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles["Finance"]["FinanceManager"],
                        PageId = pageId,
                        PermissionId = permId,
                        DepartmentId = departments["Finance"],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // FinanceSupervisor - All except Delete
                foreach (var permName in new[] { "Create", "View", "Update" })
                {
                    if (permissions.ContainsKey(permName))
                    {
                        context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                        {
                            Id = Guid.NewGuid(),
                            RoleId = departmentRoles["Finance"]["FinanceSupervisor"],
                            PageId = pageId,
                            PermissionId = permissions[permName],
                            DepartmentId = departments["Finance"],
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                // FinanceStaff - View and Create only
                foreach (var permName in new[] { "View", "Create" })
                {
                    if (permissions.ContainsKey(permName))
                    {
                        context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                        {
                            Id = Guid.NewGuid(),
                            RoleId = departmentRoles["Finance"]["FinanceStaff"],
                            PageId = pageId,
                            PermissionId = permissions[permName],
                            DepartmentId = departments["Finance"],
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                // FinanceIntern - View only
                if (permissions.ContainsKey("View"))
                {
                    context.RolePagePermissionMappings.Add(new RolePagePermissionMapping
                    {
                        Id = Guid.NewGuid(),
                        RoleId = departmentRoles["Finance"]["FinanceIntern"],
                        PageId = pageId,
                        PermissionId = permissions["View"],
                        DepartmentId = departments["Finance"],
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
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

        // Finance Department Hierarchy
        // FinanceManager (Level 0) -> FinanceSupervisor (Level 1) -> FinanceStaff (Level 2) -> FinanceIntern (Level 3)
        var financeHierarchy = new[]
        {
            ("FinanceManager", "FinanceSupervisor", 0, 1),
            ("FinanceSupervisor", "FinanceStaff", 1, 2),
            ("FinanceStaff", "FinanceIntern", 2, 3)
        };

        foreach (var (parentRole, childRole, parentLevel, childLevel) in financeHierarchy)
        {
            context.RoleHierarchies.Add(new RoleHierarchy
            {
                Id = Guid.NewGuid(),
                DepartmentId = departments["Finance"],
                ParentRoleId = departmentRoles["Finance"][parentRole],
                ChildRoleId = departmentRoles["Finance"][childRole],
                Level = childLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Marketing Department Hierarchy (similar structure)
        var marketingHierarchy = new[]
        {
            ("MarketingManager", "MarketingSupervisor", 0, 1),
            ("MarketingSupervisor", "MarketingStaff", 1, 2),
            ("MarketingStaff", "MarketingIntern", 2, 3)
        };

        foreach (var (parentRole, childRole, parentLevel, childLevel) in marketingHierarchy)
        {
            context.RoleHierarchies.Add(new RoleHierarchy
            {
                Id = Guid.NewGuid(),
                DepartmentId = departments["Marketing"],
                ParentRoleId = departmentRoles["Marketing"][parentRole],
                ChildRoleId = departmentRoles["Marketing"][childRole],
                Level = childLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Created role hierarchies");
    }

    private static async Task SeedTestUsers(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        Dictionary<string, Guid> systemRoles,
        Dictionary<string, Dictionary<string, Guid>> departmentRoles,
        Dictionary<string, Guid> departments,
        ILogger logger)
    {
        logger.LogInformation("Creating test users...");

        // 1. SuperAdmin User
        var superAdmin = new ApplicationUser
        {
            Id = FixedGuids.SuperAdminUserId,
            UserName = "superadmin@company.com",
            Email = "superadmin@company.com",
            EmailConfirmed = true,
            FirstName = "Super",
            LastName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(superAdmin, "SuperAdmin@123");
        await userManager.AddToRoleAsync(superAdmin, SystemRoles.SuperAdmin);

        // Add to UserRoleMapping
        context.UserRoleMappings.Add(new UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = superAdmin.Id,
            RoleId = systemRoles[SystemRoles.SuperAdmin],
            DepartmentId = null, // SuperAdmin has no department
            AssignedByEmail = "system@company.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // 2. Finance Admin (DepartmentAdmin for Finance)
        var financeAdmin = new ApplicationUser
        {
            Id = FixedGuids.FinanceAdminUserId,
            UserName = "financeadmin@company.com",
            Email = "financeadmin@company.com",
            EmailConfirmed = true,
            FirstName = "Finance",
            LastName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(financeAdmin, "FinanceAdmin@123");
        await userManager.AddToRoleAsync(financeAdmin, SystemRoles.DepartmentAdmin);

        context.UserRoleMappings.Add(new UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = financeAdmin.Id,
            RoleId = systemRoles[SystemRoles.DepartmentAdmin],
            DepartmentId = departments["Finance"],
            AssignedByEmail = "superadmin@company.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // 3. Finance Manager
        var financeManager = new ApplicationUser
        {
            Id = FixedGuids.FinanceManagerUserId,
            UserName = "financemanager@company.com",
            Email = "financemanager@company.com",
            EmailConfirmed = true,
            FirstName = "Finance",
            LastName = "Manager",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(financeManager, "FinanceManager@123");
        await userManager.AddToRoleAsync(financeManager, "FinanceManager");

        context.UserRoleMappings.Add(new UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = financeManager.Id,
            RoleId = departmentRoles["Finance"]["FinanceManager"],
            DepartmentId = departments["Finance"],
            AssignedByEmail = "financeadmin@company.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // 4. Finance Supervisor
        var financeSupervisor = new ApplicationUser
        {
            Id = FixedGuids.FinanceSupervisorUserId,
            UserName = "financesupervisor@company.com",
            Email = "financesupervisor@company.com",
            EmailConfirmed = true,
            FirstName = "Finance",
            LastName = "Supervisor",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(financeSupervisor, "FinanceSupervisor@123");
        await userManager.AddToRoleAsync(financeSupervisor, "FinanceSupervisor");

        context.UserRoleMappings.Add(new UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = financeSupervisor.Id,
            RoleId = departmentRoles["Finance"]["FinanceSupervisor"],
            DepartmentId = departments["Finance"],
            AssignedByEmail = "financeadmin@company.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // 5. Finance Staff
        var financeStaff = new ApplicationUser
        {
            Id = FixedGuids.FinanceStaffUserId,
            UserName = "financestaff@company.com",
            Email = "financestaff@company.com",
            EmailConfirmed = true,
            FirstName = "Finance",
            LastName = "Staff",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(financeStaff, "FinanceStaff@123");
        await userManager.AddToRoleAsync(financeStaff, "FinanceStaff");

        context.UserRoleMappings.Add(new UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = financeStaff.Id,
            RoleId = departmentRoles["Finance"]["FinanceStaff"],
            DepartmentId = departments["Finance"],
            AssignedByEmail = "financeadmin@company.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // 6. Finance Intern
        var financeIntern = new ApplicationUser
        {
            Id = FixedGuids.FinanceInternUserId,
            UserName = "financeintern@company.com",
            Email = "financeintern@company.com",
            EmailConfirmed = true,
            FirstName = "Finance",
            LastName = "Intern",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(financeIntern, "FinanceIntern@123");
        await userManager.AddToRoleAsync(financeIntern, "FinanceIntern");

        context.UserRoleMappings.Add(new UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = financeIntern.Id,
            RoleId = departmentRoles["Finance"]["FinanceIntern"],
            DepartmentId = departments["Finance"],
            AssignedByEmail = "financeadmin@company.com",
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        logger.LogInformation("Created 6 test users with proper role mappings");
    }
}