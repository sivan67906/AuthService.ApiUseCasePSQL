namespace AuthService.Tests.Unit.Domain.Constants;

/// <summary>
/// Unit tests for Permissions constants class
/// </summary>
public class PermissionsTests
{
    #region CRUD Permission Constants Tests

    [Fact]
    public void Permissions_Create_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.Create.Should().Be("Create");
    }

    [Fact]
    public void Permissions_View_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.View.Should().Be("View");
    }

    [Fact]
    public void Permissions_Update_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.Update.Should().Be("Update");
    }

    [Fact]
    public void Permissions_Delete_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.Delete.Should().Be("Delete");
    }

    [Fact]
    public void Permissions_Read_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.Read.Should().Be("Read");
    }

    #endregion

    #region Department Permission Constants Tests

    [Fact]
    public void Permissions_CreateDepartment_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.CreateDepartment.Should().Be("Department.Create");
    }

    [Fact]
    public void Permissions_ViewDepartment_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewDepartment.Should().Be("Department.View");
    }

    [Fact]
    public void Permissions_UpdateDepartment_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.UpdateDepartment.Should().Be("Department.Update");
    }

    [Fact]
    public void Permissions_DeleteDepartment_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.DeleteDepartment.Should().Be("Department.Delete");
    }

    #endregion

    #region Role Permission Constants Tests

    [Fact]
    public void Permissions_CreateRole_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.CreateRole.Should().Be("Role.Create");
    }

    [Fact]
    public void Permissions_ViewRole_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewRole.Should().Be("Role.View");
    }

    [Fact]
    public void Permissions_UpdateRole_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.UpdateRole.Should().Be("Role.Update");
    }

    [Fact]
    public void Permissions_DeleteRole_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.DeleteRole.Should().Be("Role.Delete");
    }

    #endregion

    #region Permission Management Constants Tests

    [Fact]
    public void Permissions_CreatePermission_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.CreatePermission.Should().Be("Permission.Create");
    }

    [Fact]
    public void Permissions_ViewPermission_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewPermission.Should().Be("Permission.View");
    }

    [Fact]
    public void Permissions_UpdatePermission_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.UpdatePermission.Should().Be("Permission.Update");
    }

    [Fact]
    public void Permissions_DeletePermission_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.DeletePermission.Should().Be("Permission.Delete");
    }

    #endregion

    #region Feature Permission Constants Tests

    [Fact]
    public void Permissions_CreateFeature_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.CreateFeature.Should().Be("Feature.Create");
    }

    [Fact]
    public void Permissions_ViewFeature_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewFeature.Should().Be("Feature.View");
    }

    [Fact]
    public void Permissions_UpdateFeature_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.UpdateFeature.Should().Be("Feature.Update");
    }

    [Fact]
    public void Permissions_DeleteFeature_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.DeleteFeature.Should().Be("Feature.Delete");
    }

    #endregion

    #region Page Permission Constants Tests

    [Fact]
    public void Permissions_CreatePage_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.CreatePage.Should().Be("Page.Create");
    }

    [Fact]
    public void Permissions_ViewPage_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewPage.Should().Be("Page.View");
    }

    [Fact]
    public void Permissions_UpdatePage_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.UpdatePage.Should().Be("Page.Update");
    }

    [Fact]
    public void Permissions_DeletePage_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.DeletePage.Should().Be("Page.Delete");
    }

    #endregion

    #region Mapping Permission Constants Tests

    [Fact]
    public void Permissions_ManageRoleHierarchy_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageRoleHierarchy.Should().Be("RoleHierarchy.Manage");
    }

    [Fact]
    public void Permissions_ManageUserRoleMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageUserRoleMapping.Should().Be("UserRoleMapping.Manage");
    }

    [Fact]
    public void Permissions_ManageRolePermissionMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageRolePermissionMapping.Should().Be("RolePermissionMapping.Manage");
    }

    [Fact]
    public void Permissions_ManagePagePermissionMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManagePagePermissionMapping.Should().Be("PagePermissionMapping.Manage");
    }

    [Fact]
    public void Permissions_ManagePageFeatureMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManagePageFeatureMapping.Should().Be("PageFeatureMapping.Manage");
    }

    [Fact]
    public void Permissions_ManageRoleFeatureMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageRoleFeatureMapping.Should().Be("RoleFeatureMapping.Manage");
    }

    [Fact]
    public void Permissions_ManageRoleDepartmentMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageRoleDepartmentMapping.Should().Be("RoleDepartmentMapping.Manage");
    }

    [Fact]
    public void Permissions_ManageRolePagePermissionMapping_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageRolePagePermissionMapping.Should().Be("RolePagePermissionMapping.Manage");
    }

    #endregion

    #region Account Settings Permission Constants Tests

    [Fact]
    public void Permissions_ChangePassword_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ChangePassword.Should().Be("ChangePassword");
    }

    [Fact]
    public void Permissions_ManageTwoFactor_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageTwoFactor.Should().Be("TwoFactor.Manage");
    }

    [Fact]
    public void Permissions_ManageAuthenticator_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageAuthenticator.Should().Be("Authenticator.Manage");
    }

    [Fact]
    public void Permissions_ManageAddresses_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageAddresses.Should().Be("Addresses.Manage");
    }

    #endregion

    #region User Management Permission Constants Tests

    [Fact]
    public void Permissions_ManageUsers_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ManageUsers.Should().Be("User.Manage");
    }

    [Fact]
    public void Permissions_ViewUsers_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewUsers.Should().Be("User.View");
    }

    [Fact]
    public void Permissions_AssignUserRole_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.AssignUserRole.Should().Be("UserRole.Assign");
    }

    #endregion

    #region Profile Permission Constants Tests

    [Fact]
    public void Permissions_ViewProfile_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.ViewProfile.Should().Be("Profile.View");
    }

    [Fact]
    public void Permissions_EditProfile_ShouldHaveCorrectValue()
    {
        // Assert
        Permissions.EditProfile.Should().Be("Profile.Edit");
    }

    #endregion

    #region GetAll Method Tests

    [Fact]
    public void Permissions_GetAll_ShouldReturnNonEmptyArray()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().NotBeEmpty();
    }

    [Fact]
    public void Permissions_GetAll_ShouldContainCrudPermissions()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().Contain(Permissions.Create);
        permissions.Should().Contain(Permissions.View);
        permissions.Should().Contain(Permissions.Update);
        permissions.Should().Contain(Permissions.Delete);
    }

    [Fact]
    public void Permissions_GetAll_ShouldContainDepartmentPermissions()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().Contain(Permissions.CreateDepartment);
        permissions.Should().Contain(Permissions.ViewDepartment);
        permissions.Should().Contain(Permissions.UpdateDepartment);
        permissions.Should().Contain(Permissions.DeleteDepartment);
    }

    [Fact]
    public void Permissions_GetAll_ShouldContainRolePermissions()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().Contain(Permissions.CreateRole);
        permissions.Should().Contain(Permissions.ViewRole);
        permissions.Should().Contain(Permissions.UpdateRole);
        permissions.Should().Contain(Permissions.DeleteRole);
    }

    [Fact]
    public void Permissions_GetAll_ShouldContainMappingPermissions()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().Contain(Permissions.ManageRoleHierarchy);
        permissions.Should().Contain(Permissions.ManageUserRoleMapping);
        permissions.Should().Contain(Permissions.ManageRolePermissionMapping);
    }

    [Fact]
    public void Permissions_GetAll_ShouldContainAccountSettingsPermissions()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().Contain(Permissions.ChangePassword);
        permissions.Should().Contain(Permissions.ManageTwoFactor);
        permissions.Should().Contain(Permissions.ManageAuthenticator);
    }

    [Fact]
    public void Permissions_GetAll_ShouldContainProfilePermissions()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().Contain(Permissions.ViewProfile);
        permissions.Should().Contain(Permissions.EditProfile);
    }

    [Fact]
    public void Permissions_GetAll_ShouldNotContainDuplicates()
    {
        // Act
        var permissions = Permissions.GetAll();
        var distinctPermissions = permissions.Distinct();

        // Assert
        permissions.Should().HaveCount(distinctPermissions.Count());
    }

    [Fact]
    public void Permissions_GetAll_ShouldNotContainNullOrEmpty()
    {
        // Act
        var permissions = Permissions.GetAll();

        // Assert
        permissions.Should().NotContainNulls();
        permissions.Should().NotContain(string.Empty);
    }

    #endregion

    #region Permission Naming Convention Tests

    [Fact]
    public void Permissions_EntityPermissions_ShouldFollowDotNotation()
    {
        // Assert
        Permissions.CreateDepartment.Should().Contain(".");
        Permissions.ViewDepartment.Should().Contain(".");
        Permissions.CreateRole.Should().Contain(".");
        Permissions.ViewRole.Should().Contain(".");
    }

    [Fact]
    public void Permissions_MappingPermissions_ShouldEndWithManage()
    {
        // Assert
        Permissions.ManageRoleHierarchy.Should().EndWith(".Manage");
        Permissions.ManageUserRoleMapping.Should().EndWith(".Manage");
        Permissions.ManageRolePermissionMapping.Should().EndWith(".Manage");
    }

    #endregion
}
