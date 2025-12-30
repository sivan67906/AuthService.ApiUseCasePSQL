namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for Department entity
/// </summary>
public class DepartmentTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Department_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Assert
        department.IsActive.Should().BeTrue();
        department.IsDeleted.Should().BeFalse();
        department.Description.Should().BeNull();
    }

    [Fact]
    public void Department_WhenCreated_ShouldHaveNewGuidId()
    {
        // Act
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Assert
        department.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Department_WhenCreated_ShouldHaveEmptyNavigationCollections()
    {
        // Act
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Assert
        department.Roles.Should().NotBeNull().And.BeEmpty();
        department.UserRoleMappings.Should().NotBeNull().And.BeEmpty();
        department.RoleDepartmentMappings.Should().NotBeNull().And.BeEmpty();
        department.RoleHierarchies.Should().NotBeNull().And.BeEmpty();
        department.RoleFeatureMappings.Should().NotBeNull().And.BeEmpty();
        department.RolePagePermissionMappings.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void Department_WhenCodeAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Assert
        department.Code.Should().Be("FIN");
    }

    [Fact]
    public void Department_WhenNameAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var department = new Department { Code = "FIN", Name = "Finance Department" };

        // Assert
        department.Name.Should().Be("Finance Department");
    }

    #endregion

    #region Optional Properties Tests

    [Fact]
    public void Department_WhenDescriptionAssigned_ShouldRetainValue()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        const string description = "Handles all financial operations";

        // Act
        department.Description = description;

        // Assert
        department.Description.Should().Be(description);
    }

    [Fact]
    public void Department_WhenIsActiveSetToFalse_ShouldRetainValue()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        department.IsActive = false;

        // Assert
        department.IsActive.Should().BeFalse();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void Department_WhenRoleAdded_ShouldContainRole()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var role = new ApplicationRole { Name = "Finance Manager", DepartmentId = department.Id };

        // Act
        department.Roles.Add(role);

        // Assert
        department.Roles.Should().HaveCount(1);
        department.Roles.Should().Contain(role);
    }

    [Fact]
    public void Department_WhenMultipleRolesAdded_ShouldContainAllRoles()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var role1 = new ApplicationRole { Name = "Finance Manager", DepartmentId = department.Id };
        var role2 = new ApplicationRole { Name = "Finance Analyst", DepartmentId = department.Id };
        var role3 = new ApplicationRole { Name = "Finance Staff", DepartmentId = department.Id };

        // Act
        department.Roles.Add(role1);
        department.Roles.Add(role2);
        department.Roles.Add(role3);

        // Assert
        department.Roles.Should().HaveCount(3);
    }

    [Fact]
    public void Department_WhenUserRoleMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var userRoleMapping = new UserRoleMapping
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            DepartmentId = department.Id
        };

        // Act
        department.UserRoleMappings.Add(userRoleMapping);

        // Assert
        department.UserRoleMappings.Should().HaveCount(1);
    }

    [Fact]
    public void Department_WhenRoleHierarchyAdded_ShouldContainHierarchy()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var roleHierarchy = new RoleHierarchy
        {
            DepartmentId = department.Id,
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        };

        // Act
        department.RoleHierarchies.Add(roleHierarchy);

        // Assert
        department.RoleHierarchies.Should().HaveCount(1);
    }

    [Fact]
    public void Department_WhenRoleFeatureMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var roleFeatureMapping = new RoleFeatureMapping
        {
            RoleId = Guid.NewGuid(),
            FeatureId = Guid.NewGuid(),
            DepartmentId = department.Id
        };

        // Act
        department.RoleFeatureMappings.Add(roleFeatureMapping);

        // Assert
        department.RoleFeatureMappings.Should().HaveCount(1);
    }

    [Fact]
    public void Department_WhenRolePagePermissionMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var rolePagePermissionMapping = new RolePagePermissionMapping
        {
            RoleId = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid(),
            DepartmentId = department.Id
        };

        // Act
        department.RolePagePermissionMappings.Add(rolePagePermissionMapping);

        // Assert
        department.RolePagePermissionMappings.Should().HaveCount(1);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Department_WhenCodeIsUppercase_ShouldRetainCase()
    {
        // Arrange & Act
        var department = new Department { Code = "FINANCE", Name = "Finance" };

        // Assert
        department.Code.Should().Be("FINANCE");
    }

    [Fact]
    public void Department_WhenCodeIsLowercase_ShouldRetainCase()
    {
        // Arrange & Act
        var department = new Department { Code = "finance", Name = "Finance" };

        // Assert
        department.Code.Should().Be("finance");
    }

    [Fact]
    public void Department_WhenCodeHasNumbers_ShouldAcceptValue()
    {
        // Arrange & Act
        var department = new Department { Code = "FIN01", Name = "Finance 01" };

        // Assert
        department.Code.Should().Be("FIN01");
    }

    [Fact]
    public void Department_WhenCodeHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange & Act
        var department = new Department { Code = "FIN-01_A", Name = "Finance" };

        // Assert
        department.Code.Should().Be("FIN-01_A");
    }

    [Fact]
    public void Department_WhenNameHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange & Act
        var department = new Department { Code = "RD", Name = "Research & Development" };

        // Assert
        department.Name.Should().Be("Research & Development");
    }

    [Fact]
    public void Department_WhenDescriptionIsVeryLong_ShouldAcceptValue()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var longDescription = new string('A', 5000);

        // Act
        department.Description = longDescription;

        // Assert
        department.Description.Should().HaveLength(5000);
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void Department_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        const string createdBy = "admin@example.com";

        // Act
        department.CreatedBy = createdBy;

        // Assert
        department.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void Department_WhenUpdatedAtAndModifiedBySet_ShouldRetainValues()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };
        var updatedAt = DateTime.UtcNow;
        const string modifiedBy = "user@example.com";

        // Act
        department.UpdatedAt = updatedAt;
        department.ModifiedBy = modifiedBy;

        // Assert
        department.UpdatedAt.Should().Be(updatedAt);
        department.ModifiedBy.Should().Be(modifiedBy);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void Department_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var department = new Department { Code = "FIN", Name = "Finance" };

        // Act
        department.IsDeleted = true;

        // Assert
        department.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Department_WhenSoftDeleted_OtherPropertiesShouldRemainIntact()
    {
        // Arrange
        var department = new Department
        {
            Code = "FIN",
            Name = "Finance",
            Description = "Financial Department",
            IsActive = true
        };

        // Act
        department.IsDeleted = true;

        // Assert
        department.Code.Should().Be("FIN");
        department.Name.Should().Be("Finance");
        department.Description.Should().Be("Financial Department");
        department.IsActive.Should().BeTrue();
    }

    #endregion

    #region Unique Departments Tests

    [Fact]
    public void Department_WhenTwoDepartmentsCreated_ShouldHaveUniqueIds()
    {
        // Arrange & Act
        var department1 = new Department { Code = "FIN", Name = "Finance" };
        var department2 = new Department { Code = "HR", Name = "Human Resources" };

        // Assert
        department1.Id.Should().NotBe(department2.Id);
    }

    #endregion
}
