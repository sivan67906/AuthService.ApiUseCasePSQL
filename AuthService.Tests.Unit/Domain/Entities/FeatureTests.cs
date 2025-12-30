namespace AuthService.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for Feature entity
/// </summary>
public class FeatureTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void Feature_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Assert
        feature.IsActive.Should().BeTrue();
        feature.IsDeleted.Should().BeFalse();
        feature.IsMainMenu.Should().BeFalse();
        feature.DisplayOrder.Should().Be(0);
        feature.Level.Should().Be(0);
        feature.Description.Should().BeNull();
        feature.Icon.Should().BeNull();
        feature.RouteUrl.Should().BeNull();
        feature.ParentFeatureId.Should().BeNull();
    }

    [Fact]
    public void Feature_WhenCreated_ShouldHaveNewGuidId()
    {
        // Act
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Assert
        feature.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Feature_WhenCreated_ShouldHaveEmptyNavigationCollections()
    {
        // Act
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Assert
        feature.SubFeatures.Should().NotBeNull().And.BeEmpty();
        feature.PageFeatures.Should().NotBeNull().And.BeEmpty();
        feature.RoleFeatureMappings.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void Feature_WhenCodeAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var feature = new Feature { Code = "DASHBOARD", Name = "Dashboard" };

        // Assert
        feature.Code.Should().Be("DASHBOARD");
    }

    [Fact]
    public void Feature_WhenNameAssigned_ShouldRetainValue()
    {
        // Arrange & Act
        var feature = new Feature { Code = "DASH", Name = "Main Dashboard" };

        // Assert
        feature.Name.Should().Be("Main Dashboard");
    }

    #endregion

    #region Optional Properties Tests

    [Fact]
    public void Feature_WhenDescriptionAssigned_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };
        const string description = "Main application dashboard";

        // Act
        feature.Description = description;

        // Assert
        feature.Description.Should().Be(description);
    }

    [Fact]
    public void Feature_WhenIsMainMenuSetToTrue_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.IsMainMenu = true;

        // Assert
        feature.IsMainMenu.Should().BeTrue();
    }

    [Fact]
    public void Feature_WhenDisplayOrderAssigned_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.DisplayOrder = 10;

        // Assert
        feature.DisplayOrder.Should().Be(10);
    }

    [Fact]
    public void Feature_WhenIconAssigned_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };
        const string icon = "fa-dashboard";

        // Act
        feature.Icon = icon;

        // Assert
        feature.Icon.Should().Be(icon);
    }

    [Fact]
    public void Feature_WhenRouteUrlAssigned_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };
        const string routeUrl = "/dashboard";

        // Act
        feature.RouteUrl = routeUrl;

        // Assert
        feature.RouteUrl.Should().Be(routeUrl);
    }

    [Fact]
    public void Feature_WhenLevelAssigned_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.Level = 2;

        // Assert
        feature.Level.Should().Be(2);
    }

    #endregion

    #region Parent-Child Relationship Tests

    [Fact]
    public void Feature_WhenParentFeatureIdAssigned_ShouldRetainValue()
    {
        // Arrange
        var parentFeature = new Feature { Code = "ADMIN", Name = "Administration" };
        var childFeature = new Feature { Code = "USERS", Name = "User Management" };

        // Act
        childFeature.ParentFeatureId = parentFeature.Id;

        // Assert
        childFeature.ParentFeatureId.Should().Be(parentFeature.Id);
    }

    [Fact]
    public void Feature_WhenParentFeatureAssigned_ShouldRetainReference()
    {
        // Arrange
        var parentFeature = new Feature { Code = "ADMIN", Name = "Administration" };
        var childFeature = new Feature { Code = "USERS", Name = "User Management" };

        // Act
        childFeature.ParentFeature = parentFeature;
        childFeature.ParentFeatureId = parentFeature.Id;

        // Assert
        childFeature.ParentFeature.Should().Be(parentFeature);
    }

    [Fact]
    public void Feature_WhenSubFeatureAdded_ShouldContainSubFeature()
    {
        // Arrange
        var parentFeature = new Feature { Code = "ADMIN", Name = "Administration" };
        var childFeature = new Feature { Code = "USERS", Name = "User Management", ParentFeatureId = parentFeature.Id };

        // Act
        parentFeature.SubFeatures.Add(childFeature);

        // Assert
        parentFeature.SubFeatures.Should().HaveCount(1);
        parentFeature.SubFeatures.Should().Contain(childFeature);
    }

    [Fact]
    public void Feature_WhenMultipleSubFeaturesAdded_ShouldContainAll()
    {
        // Arrange
        var parentFeature = new Feature { Code = "ADMIN", Name = "Administration" };
        var child1 = new Feature { Code = "USERS", Name = "Users", ParentFeatureId = parentFeature.Id };
        var child2 = new Feature { Code = "ROLES", Name = "Roles", ParentFeatureId = parentFeature.Id };
        var child3 = new Feature { Code = "PERMS", Name = "Permissions", ParentFeatureId = parentFeature.Id };

        // Act
        parentFeature.SubFeatures.Add(child1);
        parentFeature.SubFeatures.Add(child2);
        parentFeature.SubFeatures.Add(child3);

        // Assert
        parentFeature.SubFeatures.Should().HaveCount(3);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void Feature_WhenPageFeatureMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };
        var pageFeatureMapping = new PageFeatureMapping
        {
            FeatureId = feature.Id,
            PageId = Guid.NewGuid()
        };

        // Act
        feature.PageFeatures.Add(pageFeatureMapping);

        // Assert
        feature.PageFeatures.Should().HaveCount(1);
    }

    [Fact]
    public void Feature_WhenRoleFeatureMappingAdded_ShouldContainMapping()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };
        var roleFeatureMapping = new RoleFeatureMapping
        {
            FeatureId = feature.Id,
            RoleId = Guid.NewGuid()
        };

        // Act
        feature.RoleFeatureMappings.Add(roleFeatureMapping);

        // Assert
        feature.RoleFeatureMappings.Should().HaveCount(1);
    }

    #endregion

    #region Menu Hierarchy Tests

    [Fact]
    public void Feature_WhenMainMenuWithLevel0_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var mainMenu = new Feature
        {
            Code = "ADMIN",
            Name = "Administration",
            IsMainMenu = true,
            Level = 0,
            DisplayOrder = 1,
            Icon = "fa-cog"
        };

        // Assert
        mainMenu.IsMainMenu.Should().BeTrue();
        mainMenu.Level.Should().Be(0);
        mainMenu.ParentFeatureId.Should().BeNull();
    }

    [Fact]
    public void Feature_WhenSubMenuWithLevel1_ShouldHaveParent()
    {
        // Arrange
        var mainMenu = new Feature { Code = "ADMIN", Name = "Administration", IsMainMenu = true, Level = 0 };

        // Act
        var subMenu = new Feature
        {
            Code = "USERS",
            Name = "User Management",
            IsMainMenu = false,
            Level = 1,
            ParentFeatureId = mainMenu.Id,
            ParentFeature = mainMenu
        };

        // Assert
        subMenu.Level.Should().Be(1);
        subMenu.ParentFeatureId.Should().Be(mainMenu.Id);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Feature_WhenCodeHasSpecialCharacters_ShouldAcceptValue()
    {
        // Arrange & Act
        var feature = new Feature { Code = "ADMIN_USERS-MGT", Name = "User Management" };

        // Assert
        feature.Code.Should().Be("ADMIN_USERS-MGT");
    }

    [Fact]
    public void Feature_WhenDisplayOrderIsNegative_ShouldAcceptValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.DisplayOrder = -1;

        // Assert
        feature.DisplayOrder.Should().Be(-1);
    }

    [Fact]
    public void Feature_WhenDisplayOrderIsMaxInt_ShouldAcceptValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.DisplayOrder = int.MaxValue;

        // Assert
        feature.DisplayOrder.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Feature_WhenRouteUrlHasQueryParameters_ShouldAcceptValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };
        const string routeUrl = "/dashboard?view=grid&sort=name";

        // Act
        feature.RouteUrl = routeUrl;

        // Assert
        feature.RouteUrl.Should().Be(routeUrl);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void Feature_WhenSoftDeleted_ShouldSetIsDeletedTrue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.IsDeleted = true;

        // Assert
        feature.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Feature_WhenSoftDeleted_OtherPropertiesShouldRemainIntact()
    {
        // Arrange
        var feature = new Feature
        {
            Code = "DASH",
            Name = "Dashboard",
            IsMainMenu = true,
            IsActive = true
        };

        // Act
        feature.IsDeleted = true;

        // Assert
        feature.Code.Should().Be("DASH");
        feature.Name.Should().Be("Dashboard");
        feature.IsMainMenu.Should().BeTrue();
        feature.IsActive.Should().BeTrue();
    }

    #endregion

    #region Audit Fields Tests

    [Fact]
    public void Feature_WhenCreatedByAssigned_ShouldRetainValue()
    {
        // Arrange
        var feature = new Feature { Code = "DASH", Name = "Dashboard" };

        // Act
        feature.CreatedBy = "admin@example.com";

        // Assert
        feature.CreatedBy.Should().Be("admin@example.com");
    }

    #endregion
}
