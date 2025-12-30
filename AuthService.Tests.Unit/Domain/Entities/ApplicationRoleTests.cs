namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for ApplicationRole entity
/// </summary>
public class ApplicationRoleTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void ApplicationRole_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        role.IsActive.Should().BeTrue();
        role.IsDeleted.Should().BeFalse();
        role.Code.Should().BeNull();
        role.Description.Should().BeNull();
        role.DepartmentId.Should().BeNull();
    }

    [Fact]
    public void ApplicationRole_WhenCreated_ShouldHaveCreatedAtSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var role = new ApplicationRole();

        // Assert
        var afterCreation = DateTime.UtcNow;
        role.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        role.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void ApplicationRole_WhenCreated_ShouldHaveEmptyNavigationCollections()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        role.RolePermissions.Should().NotBeNull().And.BeEmpty();
        role.UserRoleMappings.Should().NotBeNull().And.BeEmpty();
        role.RoleDepartmentMappings.Should().NotBeNull().And.BeEmpty();
        role.ParentRoleHierarchies.Should().NotBeNull().And.BeEmpty();
        role.ChildRoleHierarchies.Should().NotBeNull().And.BeEmpty();
        role.RoleFeatureMappings.Should().NotBeNull().And.BeEmpty();
        role.RolePagePermissionMappings.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void ApplicationRole_ShouldImplementIAuditableEntity()
    {
        // Arrange
        var role = new ApplicationRole();

        // Assert
        role.Should().BeAssignableTo<IAuditableEntity>();
    }

    [Fact]
    public void ApplicationRole_ShouldImplementISoftDeletable()
    {
        // Arrange
        var role = new ApplicationRole();

        // Assert
        role.Should().BeAssignableTo<ISoftDeletable>();
    }

    #endregion

    #region Property Assignment Tests - Positive

    [Fact]
    public void ApplicationRole_WhenNameAssigned_ShouldRetainValue()
    {
        // Arrange
        var role = new ApplicationRole();
        const string name = "Administrator";

        // Act
        role.Name = name;

        // Assert
        role.Name.Should().Be(name);
    }

    [Fact]
    public void ApplicationRole_WhenCodeAssigned_ShouldRetainValue()
    {
        // Arrange
        var role = new ApplicationRole();
        const string code = "ADMIN";

        // Act
        role.Code = code;

        // Assert
        role.Code.Should().Be(code);
    }

    [Fact]
    public void ApplicationRole_WhenDescriptionAssigned_ShouldRetainValue()
    {
        // Arrange
        var role = new ApplicationRole();
        const string description = "Full administrative access";

        // Act
        role.Description = description;

        // Assert
        role.Description.Should().Be(description);
    }

    [Fact]
    public void ApplicationRole_WhenDepartmentIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var role = new ApplicationRole();
        var departmentId = Guid.NewGuid();

        // Act
        role.DepartmentId = departmentId;

        // Assert
        role.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public void ApplicationRole_WhenIsActiveSetToFalse_ShouldRetainValue()
    {
        // Arrange
        var role = new ApplicationRole();

        // Act
        role.IsActive = false;

        // Assert
        role.IsActive.Should().BeFalse();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void ApplicationRole_WhenDepartmentAssigned_ShouldRetainReference()
    {
        // Arrange
        var role = new ApplicationRole();
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        role.Department = department;
        role.DepartmentId = department.Id;

        // Assert
        role.Department.Should().Be(department);
        role.DepartmentId.Should().Be(department.Id);
    }

    [Fact]
    public void ApplicationRole_WhenRolePermissionAdded_ShouldContainMapping()
    {
        // Arrange
        var role = new ApplicationRole();
        var permissionMapping = new RolePermissionMapping
        {
            RoleId = role.Id,
            PermissionId = Guid.NewGuid()
        };

        // Act
        role.RolePermissions.Add(permissionMapping);

        // Assert
        role.RolePermissions.Should().HaveCount(1);
        role.RolePermissions.Should().Contain(permissionMapping);
    }

    [Fact]
    public void ApplicationRole_WhenUserRoleMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var role = new ApplicationRole();
        var userRoleMapping = new UserRoleMapping
        {
            RoleId = role.Id,
            UserId = Guid.NewGuid()
        };

        // Act
        role.UserRoleMappings.Add(userRoleMapping);

        // Assert
        role.UserRoleMappings.Should().HaveCount(1);
    }

    [Fact]
    public void ApplicationRole_WhenRoleFeatureMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var role = new ApplicationRole();
        var featureMapping = new RoleFeatureMapping
        {
            RoleId = role.Id,
            FeatureId = Guid.NewGuid()
        };

        // Act
        role.RoleFeatureMappings.Add(featureMapping);

        // Assert
        role.RoleFeatureMappings.Should().HaveCount(1);
    }

    [Fact]
    public void ApplicationRole_WhenRolePagePermissionMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var role = new ApplicationRole();
        var pagePermissionMapping = new RolePagePermissionMapping
        {
            RoleId = role.Id,
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid()
        };

        // Act
        role.RolePagePermissionMappings.Add(pagePermissionMapping);

        // Assert
        role.RolePagePermissionMappings.Should().HaveCount(1);
    }

    [Fact]
    public void ApplicationRole_WhenParentHierarchyAdded_ShouldContainHierarchy()
    {
        // Arrange
        var parentRole = new ApplicationRole { Name = "Manager" };
        var childRole = new ApplicationRole { Name = "Staff" };
        var hierarchy = new RoleHierarchy
        {
            ParentRoleId = parentRole.Id,
            ChildRoleId = childRole.Id,
            DepartmentId = Guid.NewGuid(),
            Level = 1
        };

        // Act
        parentRole.ChildRoleHierarchies.Add(hierarchy);
        childRole.ParentRoleHierarchies.Add(hierarchy);

        // Assert
        parentRole.ChildRoleHierarchies.Should().HaveCount(1);
        childRole.ParentRoleHierarchies.Should().HaveCount(1);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ApplicationRole_WhenCodeSetToEmptyString_ShouldAcceptValue()
    {
        // Arrange
        var role = new ApplicationRole();

        // Act
        role.Code = string.Empty;

        // Assert
        role.Code.Should().BeEmpty();
    }

    [Fact]
    public void ApplicationRole_WhenCodeSetToNull_ShouldAcceptValue()
    {
        // Arrange
        var role = new ApplicationRole { Code = "ADMIN" };

        // Act
        role.Code = null;

        // Assert
        role.Code.Should().BeNull();
    }

    [Fact]
    public void ApplicationRole_WhenNameHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange
        var role = new ApplicationRole();
        const string name = "Department-Admin_Level-1";

        // Act
        role.Name = name;

        // Assert
        role.Name.Should().Be(name);
    }

    [Fact]
    public void ApplicationRole_WhenDepartmentIdSetToNull_ShouldAcceptValue()
    {
        // Arrange
        var role = new ApplicationRole { DepartmentId = Guid.NewGuid() };

        // Act
        role.DepartmentId = null;

        // Assert
        role.DepartmentId.Should().BeNull();
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void ApplicationRole_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var role = new ApplicationRole();
        const string createdBy = "admin@example.com";

        // Act
        role.CreatedBy = createdBy;

        // Assert
        role.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void ApplicationRole_WhenUpdatedAtAndModifiedBySet_ShouldRetainValues()
    {
        // Arrange
        var role = new ApplicationRole();
        var updatedAt = DateTime.UtcNow;
        const string modifiedBy = "user@example.com";

        // Act
        role.UpdatedAt = updatedAt;
        role.ModifiedBy = modifiedBy;

        // Assert
        role.UpdatedAt.Should().Be(updatedAt);
        role.ModifiedBy.Should().Be(modifiedBy);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void ApplicationRole_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var role = new ApplicationRole { Name = "TestRole", Code = "TEST" };

        // Act
        role.IsDeleted = true;

        // Assert
        role.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void ApplicationRole_WhenSoftDeleted_OtherPropertiesShouldRemainIntact()
    {
        // Arrange
        var role = new ApplicationRole
        {
            Name = "Admin",
            Code = "ADM",
            Description = "Administrator role",
            IsActive = true
        };

        // Act
        role.IsDeleted = true;

        // Assert
        role.Name.Should().Be("Admin");
        role.Code.Should().Be("ADM");
        role.Description.Should().Be("Administrator role");
        role.IsActive.Should().BeTrue();
    }

    #endregion

    #region Multiple Mappings Tests

    [Fact]
    public void ApplicationRole_WhenMultipleMappingsAdded_ShouldContainAllMappings()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Manager" };
        var permission1 = new RolePermissionMapping { RoleId = role.Id, PermissionId = Guid.NewGuid() };
        var permission2 = new RolePermissionMapping { RoleId = role.Id, PermissionId = Guid.NewGuid() };
        var permission3 = new RolePermissionMapping { RoleId = role.Id, PermissionId = Guid.NewGuid() };

        // Act
        role.RolePermissions.Add(permission1);
        role.RolePermissions.Add(permission2);
        role.RolePermissions.Add(permission3);

        // Assert
        role.RolePermissions.Should().HaveCount(3);
    }

    #endregion
}
