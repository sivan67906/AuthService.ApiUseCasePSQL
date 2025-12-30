namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for Permission entity
/// </summary>
public class PermissionTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Permission_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var permission = new Permission { Code = "CREATE", Name = "Create" };

        // Assert
        permission.IsActive.Should().BeTrue();
        permission.IsDeleted.Should().BeFalse();
        permission.Description.Should().BeNull();
    }

    [Fact]
    public void Permission_WhenCreated_ShouldHaveNewGuidId()
    {
        // Act
        var permission = new Permission { Code = "VIEW", Name = "View" };

        // Assert
        permission.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Permission_WhenCreated_ShouldHaveEmptyNavigationCollections()
    {
        // Act
        var permission = new Permission { Code = "DELETE", Name = "Delete" };

        // Assert
        permission.RolePermissions.Should().NotBeNull().And.BeEmpty();
        permission.PagePermissions.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void Permission_WhenCodeAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var permission = new Permission { Code = "CREATE", Name = "Create" };

        // Assert
        permission.Code.Should().Be("CREATE");
    }

    [Fact]
    public void Permission_WhenNameAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var permission = new Permission { Code = "VIEW", Name = "View Records" };

        // Assert
        permission.Name.Should().Be("View Records");
    }

    #endregion

    #region Optional Properties Tests

    [Fact]
    public void Permission_WhenDescriptionAssigned_ShouldRetainValue()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };
        const string description = "Permission to create new records";

        // Act
        permission.Description = description;

        // Assert
        permission.Description.Should().Be(description);
    }

    [Fact]
    public void Permission_WhenIsActiveSetToFalse_ShouldRetainValue()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };

        // Act
        permission.IsActive = false;

        // Assert
        permission.IsActive.Should().BeFalse();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void Permission_WhenRolePermissionMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };
        var rolePermissionMapping = new RolePermissionMapping
        {
            PermissionId = permission.Id,
            RoleId = Guid.NewGuid()
        };

        // Act
        permission.RolePermissions.Add(rolePermissionMapping);

        // Assert
        permission.RolePermissions.Should().HaveCount(1);
    }

    [Fact]
    public void Permission_WhenPagePermissionMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var permission = new Permission { Code = "VIEW", Name = "View" };
        var pagePermissionMapping = new PagePermissionMapping
        {
            PermissionId = permission.Id,
            PageId = Guid.NewGuid()
        };

        // Act
        permission.PagePermissions.Add(pagePermissionMapping);

        // Assert
        permission.PagePermissions.Should().HaveCount(1);
    }

    [Fact]
    public void Permission_WhenMultipleRoleMappingsAdded_ShouldContainAll()
    {
        // Arrange
        var permission = new Permission { Code = "VIEW", Name = "View" };
        var roleMapping1 = new RolePermissionMapping { PermissionId = permission.Id, RoleId = Guid.NewGuid() };
        var roleMapping2 = new RolePermissionMapping { PermissionId = permission.Id, RoleId = Guid.NewGuid() };
        var roleMapping3 = new RolePermissionMapping { PermissionId = permission.Id, RoleId = Guid.NewGuid() };

        // Act
        permission.RolePermissions.Add(roleMapping1);
        permission.RolePermissions.Add(roleMapping2);
        permission.RolePermissions.Add(roleMapping3);

        // Assert
        permission.RolePermissions.Should().HaveCount(3);
    }

    [Fact]
    public void Permission_WhenMultiplePageMappingsAdded_ShouldContainAll()
    {
        // Arrange
        var permission = new Permission { Code = "VIEW", Name = "View" };
        var pageMapping1 = new PagePermissionMapping { PermissionId = permission.Id, PageId = Guid.NewGuid() };
        var pageMapping2 = new PagePermissionMapping { PermissionId = permission.Id, PageId = Guid.NewGuid() };

        // Act
        permission.PagePermissions.Add(pageMapping1);
        permission.PagePermissions.Add(pageMapping2);

        // Assert
        permission.PagePermissions.Should().HaveCount(2);
    }

    #endregion

    #region CRUD Permission Types Tests

    [Theory]
    [InlineData("Create", "Create")]
    [InlineData("View", "View")]
    [InlineData("Update", "Update")]
    [InlineData("Delete", "Delete")]
    [InlineData("Read", "Read")]
    public void Permission_WhenStandardCrudType_ShouldBeCreatedCorrectly(string code, string name)
    {
        // Arrange & Act
        var permission = new Permission { Code = code, Name = name };

        // Assert
        permission.Code.Should().Be(code);
        permission.Name.Should().Be(name);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Permission_WhenCodeHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange & Act
        var permission = new Permission { Code = "User.Create", Name = "Create User" };

        // Assert
        permission.Code.Should().Be("User.Create");
    }

    [Fact]
    public void Permission_WhenCodeHasNumbers_ShouldAcceptValue()
    {
        // Arrange & Act
        var permission = new Permission { Code = "PERM001", Name = "Permission 001" };

        // Assert
        permission.Code.Should().Be("PERM001");
    }

    [Fact]
    public void Permission_WhenDescriptionIsVeryLong_ShouldAcceptValue()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };
        var longDescription = new string('A', 2000);

        // Act
        permission.Description = longDescription;

        // Assert
        permission.Description.Should().HaveLength(2000);
    }

    [Fact]
    public void Permission_WhenDescriptionSetToNull_ShouldAcceptValue()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create", Description = "Some description" };

        // Act
        permission.Description = null;

        // Assert
        permission.Description.Should().BeNull();
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void Permission_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };

        // Act
        permission.IsDeleted = true;

        // Assert
        permission.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Permission_WhenSoftDeleted_OtherPropertiesShouldRemainIntact()
    {
        // Arrange
        var permission = new Permission
        {
            Code = "CREATE",
            Name = "Create",
            Description = "Create permission",
            IsActive = true
        };

        // Act
        permission.IsDeleted = true;

        // Assert
        permission.Code.Should().Be("CREATE");
        permission.Name.Should().Be("Create");
        permission.Description.Should().Be("Create permission");
        permission.IsActive.Should().BeTrue();
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void Permission_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };

        // Act
        permission.CreatedBy = "admin@example.com";

        // Assert
        permission.CreatedBy.Should().Be("admin@example.com");
    }

    [Fact]
    public void Permission_WhenUpdatedAtAndModifiedBySet_ShouldRetainValues()
    {
        // Arrange
        var permission = new Permission { Code = "CREATE", Name = "Create" };
        var updatedAt = DateTime.UtcNow;

        // Act
        permission.UpdatedAt = updatedAt;
        permission.ModifiedBy = "user@example.com";

        // Assert
        permission.UpdatedAt.Should().Be(updatedAt);
        permission.ModifiedBy.Should().Be("user@example.com");
    }

    #endregion

    #region Unique Permissions Tests

    [Fact]
    public void Permission_WhenTwoPermissionsCreated_ShouldHaveUniqueIds()
    {
        // Arrange & Act
        var permission1 = new Permission { Code = "CREATE", Name = "Create" };
        var permission2 = new Permission { Code = "DELETE", Name = "Delete" };

        // Assert
        permission1.Id.Should().NotBe(permission2.Id);
    }

    #endregion
}
