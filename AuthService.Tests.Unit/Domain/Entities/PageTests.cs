namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for Page entity
/// </summary>
public class PageTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Page_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Assert
        page.IsActive.Should().BeTrue();
        page.IsDeleted.Should().BeFalse();
        page.DisplayOrder.Should().Be(0);
        page.Description.Should().BeNull();
        page.MenuContext.Should().BeNull();
        page.ApiEndpoint.Should().BeNull();
        page.HttpMethod.Should().BeNull();
    }

    [Fact]
    public void Page_WhenCreated_ShouldHaveNewGuidId()
    {
        // Act
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Assert
        page.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Page_WhenCreated_ShouldHaveEmptyNavigationCollections()
    {
        // Act
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Assert
        page.PagePermissions.Should().NotBeNull().And.BeEmpty();
        page.PageFeatures.Should().NotBeNull().And.BeEmpty();
        page.RolePagePermissionMappings.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void Page_WhenCodeAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var page = new Page { Code = "DASHBOARD", Name = "Dashboard", Url = "/dashboard" };

        // Assert
        page.Code.Should().Be("DASHBOARD");
    }

    [Fact]
    public void Page_WhenNameAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var page = new Page { Code = "DASH", Name = "Main Dashboard", Url = "/dashboard" };

        // Assert
        page.Name.Should().Be("Main Dashboard");
    }

    [Fact]
    public void Page_WhenUrlAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard/main" };

        // Assert
        page.Url.Should().Be("/dashboard/main");
    }

    #endregion

    #region Optional Properties Tests

    [Fact]
    public void Page_WhenDescriptionAssigned_ShouldRetainValue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };
        const string description = "Main application dashboard page";

        // Act
        page.Description = description;

        // Assert
        page.Description.Should().Be(description);
    }

    [Fact]
    public void Page_WhenDisplayOrderAssigned_ShouldRetainValue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Act
        page.DisplayOrder = 5;

        // Assert
        page.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Page_WhenMenuContextAssigned_ShouldRetainValue()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };
        const string menuContext = "Administration/UserManagement";

        // Act
        page.MenuContext = menuContext;

        // Assert
        page.MenuContext.Should().Be(menuContext);
    }

    [Fact]
    public void Page_WhenApiEndpointAssigned_ShouldRetainValue()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };
        const string apiEndpoint = "/api/users";

        // Act
        page.ApiEndpoint = apiEndpoint;

        // Assert
        page.ApiEndpoint.Should().Be(apiEndpoint);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public void Page_WhenHttpMethodAssigned_ShouldRetainValue(string httpMethod)
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };

        // Act
        page.HttpMethod = httpMethod;

        // Assert
        page.HttpMethod.Should().Be(httpMethod);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void Page_WhenPagePermissionMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };
        var permissionMapping = new PagePermissionMapping
        {
            PageId = page.Id,
            PermissionId = Guid.NewGuid()
        };

        // Act
        page.PagePermissions.Add(permissionMapping);

        // Assert
        page.PagePermissions.Should().HaveCount(1);
    }

    [Fact]
    public void Page_WhenPageFeatureMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };
        var featureMapping = new PageFeatureMapping
        {
            PageId = page.Id,
            FeatureId = Guid.NewGuid()
        };

        // Act
        page.PageFeatures.Add(featureMapping);

        // Assert
        page.PageFeatures.Should().HaveCount(1);
    }

    [Fact]
    public void Page_WhenRolePagePermissionMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };
        var rolePagePermissionMapping = new RolePagePermissionMapping
        {
            PageId = page.Id,
            RoleId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid()
        };

        // Act
        page.RolePagePermissionMappings.Add(rolePagePermissionMapping);

        // Assert
        page.RolePagePermissionMappings.Should().HaveCount(1);
    }

    [Fact]
    public void Page_WhenMultiplePermissionsAdded_ShouldContainAll()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };
        var createPermission = new PagePermissionMapping { PageId = page.Id, PermissionId = Guid.NewGuid() };
        var viewPermission = new PagePermissionMapping { PageId = page.Id, PermissionId = Guid.NewGuid() };
        var updatePermission = new PagePermissionMapping { PageId = page.Id, PermissionId = Guid.NewGuid() };
        var deletePermission = new PagePermissionMapping { PageId = page.Id, PermissionId = Guid.NewGuid() };

        // Act
        page.PagePermissions.Add(createPermission);
        page.PagePermissions.Add(viewPermission);
        page.PagePermissions.Add(updatePermission);
        page.PagePermissions.Add(deletePermission);

        // Assert
        page.PagePermissions.Should().HaveCount(4);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Page_WhenUrlHasQueryParameters_ShouldAcceptValue()
    {
        // Arrange & Act
        var page = new Page { Code = "REPORT", Name = "Report", Url = "/reports/sales?year=2024&month=12" };

        // Assert
        page.Url.Should().Be("/reports/sales?year=2024&month=12");
    }

    [Fact]
    public void Page_WhenUrlHasHashFragment_ShouldAcceptValue()
    {
        // Arrange & Act
        var page = new Page { Code = "DOC", Name = "Documentation", Url = "/docs#section-1" };

        // Assert
        page.Url.Should().Be("/docs#section-1");
    }

    [Fact]
    public void Page_WhenCodeHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange & Act
        var page = new Page { Code = "USER_MGMT-01", Name = "Users", Url = "/users" };

        // Assert
        page.Code.Should().Be("USER_MGMT-01");
    }

    [Fact]
    public void Page_WhenDisplayOrderIsNegative_ShouldAcceptValue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Act
        page.DisplayOrder = -10;

        // Assert
        page.DisplayOrder.Should().Be(-10);
    }

    [Fact]
    public void Page_WhenApiEndpointHasVersioning_ShouldAcceptValue()
    {
        // Arrange
        var page = new Page { Code = "USERS", Name = "Users", Url = "/admin/users" };

        // Act
        page.ApiEndpoint = "/api/v2/users";

        // Assert
        page.ApiEndpoint.Should().Be("/api/v2/users");
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void Page_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Act
        page.IsDeleted = true;

        // Assert
        page.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Page_WhenSoftDeleted_OtherPropertiesShouldRemainIntact()
    {
        // Arrange
        var page = new Page
        {
            Code = "DASH",
            Name = "Dashboard",
            Url = "/dashboard",
            Description = "Main dashboard",
            IsActive = true
        };

        // Act
        page.IsDeleted = true;

        // Assert
        page.Code.Should().Be("DASH");
        page.Name.Should().Be("Dashboard");
        page.Url.Should().Be("/dashboard");
        page.IsActive.Should().BeTrue();
    }

    #endregion

    #region IsActive Toggle Tests

    [Fact]
    public void Page_WhenIsActiveSetToFalse_ShouldRetainValue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Act
        page.IsActive = false;

        // Assert
        page.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Page_WhenIsActiveToggledMultipleTimes_ShouldRetainLatestValue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Act
        page.IsActive = false;
        page.IsActive = true;
        page.IsActive = false;

        // Assert
        page.IsActive.Should().BeFalse();
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void Page_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };

        // Act
        page.CreatedBy = "admin@example.com";

        // Assert
        page.CreatedBy.Should().Be("admin@example.com");
    }

    [Fact]
    public void Page_WhenUpdatedAtAndModifiedBySet_ShouldRetainValues()
    {
        // Arrange
        var page = new Page { Code = "DASH", Name = "Dashboard", Url = "/dashboard" };
        var updatedAt = DateTime.UtcNow;

        // Act
        page.UpdatedAt = updatedAt;
        page.ModifiedBy = "user@example.com";

        // Assert
        page.UpdatedAt.Should().Be(updatedAt);
        page.ModifiedBy.Should().Be("user@example.com");
    }

    #endregion
}
