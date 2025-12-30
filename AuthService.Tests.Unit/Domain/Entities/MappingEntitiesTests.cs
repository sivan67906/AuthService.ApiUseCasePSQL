namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for UserRoleMapping entity
/// </summary>
public class UserRoleMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void UserRoleMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new UserRoleMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
        mapping.AssignedByEmail.Should().BeEmpty();
        mapping.DepartmentId.Should().BeNull();
    }

    [Fact]
    public void UserRoleMapping_WhenCreated_ShouldHaveAssignedAtSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var mapping = new UserRoleMapping();

        // Assert
        var afterCreation = DateTime.UtcNow;
        mapping.AssignedAt.Should().BeOnOrAfter(beforeCreation);
        mapping.AssignedAt.Should().BeOnOrBefore(afterCreation);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void UserRoleMapping_WhenUserIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        var userId = Guid.NewGuid();

        // Act
        mapping.UserId = userId;

        // Assert
        mapping.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserRoleMapping_WhenRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        var roleId = Guid.NewGuid();

        // Act
        mapping.RoleId = roleId;

        // Assert
        mapping.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void UserRoleMapping_WhenDepartmentIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        var departmentId = Guid.NewGuid();

        // Act
        mapping.DepartmentId = departmentId;

        // Assert
        mapping.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public void UserRoleMapping_WhenAssignedByEmailAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        const string email = "admin@example.com";

        // Act
        mapping.AssignedByEmail = email;

        // Assert
        mapping.AssignedByEmail.Should().Be(email);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void UserRoleMapping_WhenUserAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        var user = new ApplicationUser { Email = "test@example.com" };

        // Act
        mapping.User = user;
        mapping.UserId = user.Id;

        // Assert
        mapping.User.Should().Be(user);
    }

    [Fact]
    public void UserRoleMapping_WhenRoleAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        var role = new ApplicationRole { Name = "Admin" };

        // Act
        mapping.Role = role;
        mapping.RoleId = role.Id;

        // Assert
        mapping.Role.Should().Be(role);
    }

    [Fact]
    public void UserRoleMapping_WhenDepartmentAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new UserRoleMapping();
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        mapping.Department = department;
        mapping.DepartmentId = department.Id;

        // Assert
        mapping.Department.Should().Be(department);
    }

    #endregion

    #region SuperAdmin Scenario Tests

    [Fact]
    public void UserRoleMapping_ForSuperAdmin_ShouldHaveNullDepartmentId()
    {
        // Arrange & Act
        var mapping = new UserRoleMapping
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = null // SuperAdmin has no department
        };

        // Assert
        mapping.DepartmentId.Should().BeNull();
    }

    #endregion
}

/// <summary>
/// Unit tests for RoleHierarchy entity
/// </summary>
public class RoleHierarchyTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void RoleHierarchy_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var hierarchy = new RoleHierarchy();

        // Assert
        hierarchy.IsActive.Should().BeTrue();
        hierarchy.IsDeleted.Should().BeFalse();
        hierarchy.Level.Should().Be(0);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void RoleHierarchy_WhenDepartmentIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var hierarchy = new RoleHierarchy();
        var departmentId = Guid.NewGuid();

        // Act
        hierarchy.DepartmentId = departmentId;

        // Assert
        hierarchy.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public void RoleHierarchy_WhenParentRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var hierarchy = new RoleHierarchy();
        var parentRoleId = Guid.NewGuid();

        // Act
        hierarchy.ParentRoleId = parentRoleId;

        // Assert
        hierarchy.ParentRoleId.Should().Be(parentRoleId);
    }

    [Fact]
    public void RoleHierarchy_WhenChildRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var hierarchy = new RoleHierarchy();
        var childRoleId = Guid.NewGuid();

        // Act
        hierarchy.ChildRoleId = childRoleId;

        // Assert
        hierarchy.ChildRoleId.Should().Be(childRoleId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void RoleHierarchy_WhenLevelAssigned_ShouldRetainValue(int level)
    {
        // Arrange
        var hierarchy = new RoleHierarchy();

        // Act
        hierarchy.Level = level;

        // Assert
        hierarchy.Level.Should().Be(level);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void RoleHierarchy_WhenDepartmentAssigned_ShouldRetainReference()
    {
        // Arrange
        var hierarchy = new RoleHierarchy();
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        hierarchy.Department = department;
        hierarchy.DepartmentId = department.Id;

        // Assert
        hierarchy.Department.Should().Be(department);
    }

    [Fact]
    public void RoleHierarchy_WhenParentRoleAssigned_ShouldRetainReference()
    {
        // Arrange
        var hierarchy = new RoleHierarchy();
        var parentRole = new ApplicationRole { Name = "Manager" };

        // Act
        hierarchy.ParentRole = parentRole;
        hierarchy.ParentRoleId = parentRole.Id;

        // Assert
        hierarchy.ParentRole.Should().Be(parentRole);
    }

    [Fact]
    public void RoleHierarchy_WhenChildRoleAssigned_ShouldRetainReference()
    {
        // Arrange
        var hierarchy = new RoleHierarchy();
        var childRole = new ApplicationRole { Name = "Staff" };

        // Act
        hierarchy.ChildRole = childRole;
        hierarchy.ChildRoleId = childRole.Id;

        // Assert
        hierarchy.ChildRole.Should().Be(childRole);
    }

    #endregion

    #region Hierarchy Structure Tests

    [Fact]
    public void RoleHierarchy_WhenCompleteHierarchySet_ShouldRetainAllValues()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var managerRole = new ApplicationRole { Name = "Finance Manager" };
        var staffRole = new ApplicationRole { Name = "Finance Staff" };

        // Act
        var hierarchy = new RoleHierarchy
        {
            DepartmentId = department.Id,
            Department = department,
            ParentRoleId = managerRole.Id,
            ParentRole = managerRole,
            ChildRoleId = staffRole.Id,
            ChildRole = staffRole,
            Level = 1,
            IsActive = true
        };

        // Assert
        hierarchy.DepartmentId.Should().Be(department.Id);
        hierarchy.ParentRoleId.Should().Be(managerRole.Id);
        hierarchy.ChildRoleId.Should().Be(staffRole.Id);
        hierarchy.Level.Should().Be(1);
    }

    #endregion
}

/// <summary>
/// Unit tests for RoleFeatureMapping entity
/// </summary>
public class RoleFeatureMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void RoleFeatureMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new RoleFeatureMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
        mapping.DepartmentId.Should().BeNull();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void RoleFeatureMapping_WhenRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RoleFeatureMapping();
        var roleId = Guid.NewGuid();

        // Act
        mapping.RoleId = roleId;

        // Assert
        mapping.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void RoleFeatureMapping_WhenFeatureIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RoleFeatureMapping();
        var featureId = Guid.NewGuid();

        // Act
        mapping.FeatureId = featureId;

        // Assert
        mapping.FeatureId.Should().Be(featureId);
    }

    [Fact]
    public void RoleFeatureMapping_WhenDepartmentIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RoleFeatureMapping();
        var departmentId = Guid.NewGuid();

        // Act
        mapping.DepartmentId = departmentId;

        // Assert
        mapping.DepartmentId.Should().Be(departmentId);
    }

    #endregion

    #region SuperAdmin Scenario Tests

    [Fact]
    public void RoleFeatureMapping_ForSuperAdmin_ShouldHaveNullDepartmentId()
    {
        // Arrange & Act
        var mapping = new RoleFeatureMapping
        {
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = null // SuperAdmin has no department
        };

        // Assert
        mapping.DepartmentId.Should().BeNull();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void RoleFeatureMapping_WhenRoleAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new RoleFeatureMapping();
        var role = new ApplicationRole { Name = "Admin" };

        // Act
        mapping.Role = role;
        mapping.RoleId = role.Id;

        // Assert
        mapping.Role.Should().Be(role);
    }

    [Fact]
    public void RoleFeatureMapping_WhenFeatureAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new RoleFeatureMapping();
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        mapping.Feature = feature;
        mapping.FeatureId = feature.Id;

        // Assert
        mapping.Feature.Should().Be(feature);
    }

    #endregion
}

/// <summary>
/// Unit tests for RolePagePermissionMapping entity
/// </summary>
public class RolePagePermissionMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void RolePagePermissionMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new RolePagePermissionMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
        mapping.DepartmentId.Should().BeNull();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void RolePagePermissionMapping_WhenRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RolePagePermissionMapping();
        var roleId = Guid.NewGuid();

        // Act
        mapping.RoleId = roleId;

        // Assert
        mapping.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void RolePagePermissionMapping_WhenPageIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RolePagePermissionMapping();
        var pageId = Guid.NewGuid();

        // Act
        mapping.PageId = pageId;

        // Assert
        mapping.PageId.Should().Be(pageId);
    }

    [Fact]
    public void RolePagePermissionMapping_WhenPermissionIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RolePagePermissionMapping();
        var permissionId = Guid.NewGuid();

        // Act
        mapping.PermissionId = permissionId;

        // Assert
        mapping.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void RolePagePermissionMapping_WhenDepartmentIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RolePagePermissionMapping();
        var departmentId = Guid.NewGuid();

        // Act
        mapping.DepartmentId = departmentId;

        // Assert
        mapping.DepartmentId.Should().Be(departmentId);
    }

    #endregion

    #region Complete Mapping Tests

    [Fact]
    public void RolePagePermissionMapping_WhenAllPropertiesSet_ShouldRetainAllValues()
    {
        // Arrange & Act
        var mapping = new RolePagePermissionMapping
        {
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            IsActive = true
        };

        // Assert
        mapping.RoleId.Should().NotBe(Guid.Empty);
        mapping.PageId.Should().NotBe(Guid.Empty);
        mapping.PermissionId.Should().NotBe(Guid.Empty);
        mapping.DepartmentId.Should().NotBeNull();
        mapping.IsActive.Should().BeTrue();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void RolePagePermissionMapping_WhenAllNavigationsSet_ShouldRetainReferences()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Admin" };
        var page = new Page { Code = "USERS", Name = "Users", Url = "/users" };
        var permission = new Permission { Code = "CREATE", Name = "Create" };
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        var mapping = new RolePagePermissionMapping
        {
            RoleId = role.Id,
            Role = role,
            PageId = page.Id,
            Page = page,
            PermissionId = permission.Id,
            Permission = permission,
            DepartmentId = department.Id,
            Department = department
        };

        // Assert
        mapping.Role.Should().Be(role);
        mapping.Page.Should().Be(page);
        mapping.Permission.Should().Be(permission);
        mapping.Department.Should().Be(department);
    }

    #endregion
}

/// <summary>
/// Unit tests for PageFeatureMapping entity
/// </summary>
public class PageFeatureMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void PageFeatureMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new PageFeatureMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void PageFeatureMapping_WhenPageIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new PageFeatureMapping();
        var pageId = Guid.NewGuid();

        // Act
        mapping.PageId = pageId;

        // Assert
        mapping.PageId.Should().Be(pageId);
    }

    [Fact]
    public void PageFeatureMapping_WhenFeatureIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new PageFeatureMapping();
        var featureId = Guid.NewGuid();

        // Act
        mapping.FeatureId = featureId;

        // Assert
        mapping.FeatureId.Should().Be(featureId);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void PageFeatureMapping_WhenPageAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new PageFeatureMapping();
        var page = new Page { Code = "USERS", Name = "Users", Url = "/users" };

        // Act
        mapping.Page = page;
        mapping.PageId = page.Id;

        // Assert
        mapping.Page.Should().Be(page);
    }

    [Fact]
    public void PageFeatureMapping_WhenFeatureAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new PageFeatureMapping();
        var feature = new Feature { Code = "USER_MGMT", Name = "User Management" };

        // Act
        mapping.Feature = feature;
        mapping.FeatureId = feature.Id;

        // Assert
        mapping.Feature.Should().Be(feature);
    }

    #endregion
}

/// <summary>
/// Unit tests for PagePermissionMapping entity
/// </summary>
public class PagePermissionMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void PagePermissionMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new PagePermissionMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void PagePermissionMapping_WhenPageIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new PagePermissionMapping();
        var pageId = Guid.NewGuid();

        // Act
        mapping.PageId = pageId;

        // Assert
        mapping.PageId.Should().Be(pageId);
    }

    [Fact]
    public void PagePermissionMapping_WhenPermissionIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new PagePermissionMapping();
        var permissionId = Guid.NewGuid();

        // Act
        mapping.PermissionId = permissionId;

        // Assert
        mapping.PermissionId.Should().Be(permissionId);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void PagePermissionMapping_WhenPageAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new PagePermissionMapping();
        var page = new Page { Code = "USERS", Name = "Users", Url = "/users" };

        // Act
        mapping.Page = page;
        mapping.PageId = page.Id;

        // Assert
        mapping.Page.Should().Be(page);
    }

    [Fact]
    public void PagePermissionMapping_WhenPermissionAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new PagePermissionMapping();
        var permission = new Permission { Code = "VIEW", Name = "View" };

        // Act
        mapping.Permission = permission;
        mapping.PermissionId = permission.Id;

        // Assert
        mapping.Permission.Should().Be(permission);
    }

    #endregion
}

/// <summary>
/// Unit tests for RolePermissionMapping entity
/// </summary>
public class RolePermissionMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void RolePermissionMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new RolePermissionMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void RolePermissionMapping_WhenRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RolePermissionMapping();
        var roleId = Guid.NewGuid();

        // Act
        mapping.RoleId = roleId;

        // Assert
        mapping.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void RolePermissionMapping_WhenPermissionIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RolePermissionMapping();
        var permissionId = Guid.NewGuid();

        // Act
        mapping.PermissionId = permissionId;

        // Assert
        mapping.PermissionId.Should().Be(permissionId);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void RolePermissionMapping_WhenRoleAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new RolePermissionMapping();
        var role = new ApplicationRole { Name = "Admin" };

        // Act
        mapping.Role = role;
        mapping.RoleId = role.Id;

        // Assert
        mapping.Role.Should().Be(role);
    }

    [Fact]
    public void RolePermissionMapping_WhenPermissionAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new RolePermissionMapping();
        var permission = new Permission { Code = "CREATE", Name = "Create" };

        // Act
        mapping.Permission = permission;
        mapping.PermissionId = permission.Id;

        // Assert
        mapping.Permission.Should().Be(permission);
    }

    #endregion
}

/// <summary>
/// Unit tests for RoleDepartmentMapping entity
/// </summary>
public class RoleDepartmentMappingTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void RoleDepartmentMapping_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var mapping = new RoleDepartmentMapping();

        // Assert
        mapping.IsActive.Should().BeTrue();
        mapping.IsDeleted.Should().BeFalse();
        mapping.IsPrimary.Should().BeFalse();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void RoleDepartmentMapping_WhenRoleIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RoleDepartmentMapping();
        var roleId = Guid.NewGuid();

        // Act
        mapping.RoleId = roleId;

        // Assert
        mapping.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void RoleDepartmentMapping_WhenDepartmentIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RoleDepartmentMapping();
        var departmentId = Guid.NewGuid();

        // Act
        mapping.DepartmentId = departmentId;

        // Assert
        mapping.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public void RoleDepartmentMapping_WhenIsPrimarySetToTrue_ShouldRetainValue()
    {
        // Arrange
        var mapping = new RoleDepartmentMapping();

        // Act
        mapping.IsPrimary = true;

        // Assert
        mapping.IsPrimary.Should().BeTrue();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void RoleDepartmentMapping_WhenRoleAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new RoleDepartmentMapping();
        var role = new ApplicationRole { Name = "Admin" };

        // Act
        mapping.Role = role;
        mapping.RoleId = role.Id;

        // Assert
        mapping.Role.Should().Be(role);
    }

    [Fact]
    public void RoleDepartmentMapping_WhenDepartmentAssigned_ShouldRetainReference()
    {
        // Arrange
        var mapping = new RoleDepartmentMapping();
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        mapping.Department = department;
        mapping.DepartmentId = department.Id;

        // Assert
        mapping.Department.Should().Be(department);
    }

    #endregion
}
