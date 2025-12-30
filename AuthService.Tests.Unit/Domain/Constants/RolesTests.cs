namespace AuthService.Tests.Unit.Domain.Constants;

/// <summary>
/// Unit tests for Roles constants class
/// </summary>
public class RolesTests
{
    #region Admin Roles Constants Tests

    [Fact]
    public void Roles_Admin_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Admin.Should().Be("Admin");
    }

    [Fact]
    public void Roles_FinanceAdmin_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.FinanceAdmin.Should().Be("FinanceAdmin");
    }

    [Fact]
    public void Roles_HRAdmin_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.HRAdmin.Should().Be("HRAdmin");
    }

    [Fact]
    public void Roles_ITAdmin_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.ITAdmin.Should().Be("ITAdmin");
    }

    #endregion

    #region Manager Roles Constants Tests

    [Fact]
    public void Roles_Manager_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Manager.Should().Be("Manager");
    }

    [Fact]
    public void Roles_FinanceManager_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.FinanceManager.Should().Be("FinanceManager");
    }

    [Fact]
    public void Roles_HRManager_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.HRManager.Should().Be("HRManager");
    }

    [Fact]
    public void Roles_ITManager_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.ITManager.Should().Be("ITManager");
    }

    [Fact]
    public void Roles_MarketingManager_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.MarketingManager.Should().Be("MarketingManager");
    }

    #endregion

    #region Supervisor Roles Constants Tests

    [Fact]
    public void Roles_FinanceSupervisor_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.FinanceSupervisor.Should().Be("FinanceSupervisor");
    }

    [Fact]
    public void Roles_MarketingSupervisor_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.MarketingSupervisor.Should().Be("MarketingSupervisor");
    }

    #endregion

    #region Analyst/Executive Roles Constants Tests

    [Fact]
    public void Roles_Analyst_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Analyst.Should().Be("Analyst");
    }

    [Fact]
    public void Roles_Executive_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Executive.Should().Be("Executive");
    }

    [Fact]
    public void Roles_FinanceAnalyst_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.FinanceAnalyst.Should().Be("FinanceAnalyst");
    }

    [Fact]
    public void Roles_HRAnalyst_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.HRAnalyst.Should().Be("HRAnalyst");
    }

    #endregion

    #region Staff Roles Constants Tests

    [Fact]
    public void Roles_Staff_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Staff.Should().Be("Staff");
    }

    [Fact]
    public void Roles_FinanceStaff_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.FinanceStaff.Should().Be("FinanceStaff");
    }

    [Fact]
    public void Roles_HRStaff_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.HRStaff.Should().Be("HRStaff");
    }

    [Fact]
    public void Roles_MarketingStaff_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.MarketingStaff.Should().Be("MarketingStaff");
    }

    #endregion

    #region Intern Roles Constants Tests

    [Fact]
    public void Roles_FinanceIntern_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.FinanceIntern.Should().Be("FinanceIntern");
    }

    [Fact]
    public void Roles_MarketingIntern_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.MarketingIntern.Should().Be("MarketingIntern");
    }

    #endregion

    #region Legacy Roles Constants Tests

    [Fact]
    public void Roles_User_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.User.Should().Be("User");
    }

    [Fact]
    public void Roles_Accountant_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Accountant.Should().Be("Accountant");
    }

    [Fact]
    public void Roles_Auditor_ShouldHaveCorrectValue()
    {
        // Assert
        Roles.Auditor.Should().Be("Auditor");
    }

    #endregion

    #region GetAllDepartmentRoles Method Tests

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldReturnNonEmptyArray()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();

        // Assert
        roles.Should().NotBeEmpty();
    }

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldContainFinanceRoles()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();

        // Assert
        roles.Should().Contain(Roles.FinanceManager);
        roles.Should().Contain(Roles.FinanceSupervisor);
        roles.Should().Contain(Roles.FinanceStaff);
        roles.Should().Contain(Roles.FinanceIntern);
    }

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldContainMarketingRoles()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();

        // Assert
        roles.Should().Contain(Roles.MarketingManager);
        roles.Should().Contain(Roles.MarketingSupervisor);
        roles.Should().Contain(Roles.MarketingStaff);
        roles.Should().Contain(Roles.MarketingIntern);
    }

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldHaveEightRoles()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();

        // Assert
        roles.Should().HaveCount(8);
    }

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldNotContainDuplicates()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();
        var distinctRoles = roles.Distinct();

        // Assert
        roles.Should().HaveCount(distinctRoles.Count());
    }

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldNotContainNullOrEmpty()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();

        // Assert
        roles.Should().NotContainNulls();
        roles.Should().NotContain(string.Empty);
    }

    [Fact]
    public void Roles_GetAllDepartmentRoles_ShouldNotContainGenericRoles()
    {
        // Act
        var roles = Roles.GetAllDepartmentRoles();

        // Assert - Generic roles like Admin, Manager, Staff should not be in department-specific list
        roles.Should().NotContain(Roles.Admin);
        roles.Should().NotContain(Roles.Manager);
        roles.Should().NotContain(Roles.Staff);
    }

    #endregion

    #region Role Naming Convention Tests

    [Fact]
    public void Roles_DepartmentRoles_ShouldStartWithDepartmentName()
    {
        // Assert
        Roles.FinanceManager.Should().StartWith("Finance");
        Roles.FinanceSupervisor.Should().StartWith("Finance");
        Roles.MarketingManager.Should().StartWith("Marketing");
        Roles.HRAdmin.Should().StartWith("HR");
    }

    [Fact]
    public void Roles_AdminRoles_ShouldEndWithAdmin()
    {
        // Assert
        Roles.FinanceAdmin.Should().EndWith("Admin");
        Roles.HRAdmin.Should().EndWith("Admin");
        Roles.ITAdmin.Should().EndWith("Admin");
    }

    [Fact]
    public void Roles_ManagerRoles_ShouldEndWithManager()
    {
        // Assert
        Roles.FinanceManager.Should().EndWith("Manager");
        Roles.HRManager.Should().EndWith("Manager");
        Roles.ITManager.Should().EndWith("Manager");
        Roles.MarketingManager.Should().EndWith("Manager");
    }

    #endregion
}

/// <summary>
/// Unit tests for SystemRoles constants class
/// </summary>
public class SystemRolesTests
{
    #region System Role Constants Tests

    [Fact]
    public void SystemRoles_SuperAdmin_ShouldHaveCorrectValue()
    {
        // Assert
        SystemRoles.SuperAdmin.Should().Be("SuperAdmin");
    }

    [Fact]
    public void SystemRoles_DepartmentAdmin_ShouldHaveCorrectValue()
    {
        // Assert
        SystemRoles.DepartmentAdmin.Should().Be("DepartmentAdmin");
    }

    [Fact]
    public void SystemRoles_PendingUser_ShouldHaveCorrectValue()
    {
        // Assert
        SystemRoles.PendingUser.Should().Be("PendingUser");
    }

    #endregion

    #region GetAll Method Tests

    [Fact]
    public void SystemRoles_GetAll_ShouldReturnThreeRoles()
    {
        // Act
        var roles = SystemRoles.GetAll();

        // Assert
        roles.Should().HaveCount(3);
    }

    [Fact]
    public void SystemRoles_GetAll_ShouldContainSuperAdmin()
    {
        // Act
        var roles = SystemRoles.GetAll();

        // Assert
        roles.Should().Contain(SystemRoles.SuperAdmin);
    }

    [Fact]
    public void SystemRoles_GetAll_ShouldContainDepartmentAdmin()
    {
        // Act
        var roles = SystemRoles.GetAll();

        // Assert
        roles.Should().Contain(SystemRoles.DepartmentAdmin);
    }

    [Fact]
    public void SystemRoles_GetAll_ShouldContainPendingUser()
    {
        // Act
        var roles = SystemRoles.GetAll();

        // Assert
        roles.Should().Contain(SystemRoles.PendingUser);
    }

    [Fact]
    public void SystemRoles_GetAll_ShouldNotContainDuplicates()
    {
        // Act
        var roles = SystemRoles.GetAll();
        var distinctRoles = roles.Distinct();

        // Assert
        roles.Should().HaveCount(distinctRoles.Count());
    }

    [Fact]
    public void SystemRoles_GetAll_ShouldNotContainNullOrEmpty()
    {
        // Act
        var roles = SystemRoles.GetAll();

        // Assert
        roles.Should().NotContainNulls();
        roles.Should().NotContain(string.Empty);
    }

    #endregion

    #region IsSystemRole Method Tests

    [Fact]
    public void SystemRoles_IsSystemRole_SuperAdmin_ShouldReturnTrue()
    {
        // Act
        var result = SystemRoles.IsSystemRole(SystemRoles.SuperAdmin);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_DepartmentAdmin_ShouldReturnTrue()
    {
        // Act
        var result = SystemRoles.IsSystemRole(SystemRoles.DepartmentAdmin);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_PendingUser_ShouldReturnTrue()
    {
        // Act
        var result = SystemRoles.IsSystemRole(SystemRoles.PendingUser);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_RegularRole_ShouldReturnFalse()
    {
        // Act
        var result = SystemRoles.IsSystemRole(Roles.Admin);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_FinanceManager_ShouldReturnFalse()
    {
        // Act
        var result = SystemRoles.IsSystemRole(Roles.FinanceManager);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_Staff_ShouldReturnFalse()
    {
        // Act
        var result = SystemRoles.IsSystemRole(Roles.Staff);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_EmptyString_ShouldReturnFalse()
    {
        // Act
        var result = SystemRoles.IsSystemRole(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_RandomString_ShouldReturnFalse()
    {
        // Act
        var result = SystemRoles.IsSystemRole("RandomRole123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_CaseSensitive_ShouldReturnFalseForWrongCase()
    {
        // Act
        var result = SystemRoles.IsSystemRole("superadmin"); // lowercase

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_CaseSensitive_ShouldReturnFalseForUpperCase()
    {
        // Act
        var result = SystemRoles.IsSystemRole("SUPERADMIN"); // uppercase

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void SystemRoles_IsSystemRole_PartialMatch_ShouldReturnFalse()
    {
        // Act
        var result1 = SystemRoles.IsSystemRole("Super");
        var result2 = SystemRoles.IsSystemRole("Admin");
        var result3 = SystemRoles.IsSystemRole("Pending");

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    [Fact]
    public void SystemRoles_IsSystemRole_WithExtraSpaces_ShouldReturnFalse()
    {
        // Act
        var result1 = SystemRoles.IsSystemRole(" SuperAdmin");
        var result2 = SystemRoles.IsSystemRole("SuperAdmin ");
        var result3 = SystemRoles.IsSystemRole(" SuperAdmin ");

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    #endregion

    #region System Role Hierarchy Tests

    [Fact]
    public void SystemRoles_SuperAdmin_ShouldBeHighestPrivilegeRole()
    {
        // This is a documentation test - SuperAdmin should have the highest privilege
        // Assert
        SystemRoles.SuperAdmin.Should().Be("SuperAdmin");
    }

    [Fact]
    public void SystemRoles_PendingUser_ShouldBeLowestPrivilegeSystemRole()
    {
        // This is a documentation test - PendingUser should have the lowest privilege
        // Assert
        SystemRoles.PendingUser.Should().Be("PendingUser");
    }

    #endregion

    #region All System Roles Validation Tests

    [Theory]
    [InlineData("SuperAdmin", true)]
    [InlineData("DepartmentAdmin", true)]
    [InlineData("PendingUser", true)]
    [InlineData("Admin", false)]
    [InlineData("Manager", false)]
    [InlineData("User", false)]
    [InlineData("", false)]
    [InlineData("superadmin", false)]
    public void SystemRoles_IsSystemRole_VariousInputs_ShouldReturnExpectedResult(string roleName, bool expected)
    {
        // Act
        var result = SystemRoles.IsSystemRole(roleName);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
