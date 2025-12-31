using AuthService.Api.Authorization;

namespace AuthService.Tests.Unit.Api.Controllers;

public class AuthorizationAttributesTests
{
    [Fact]
    public void RoleAuthorizationAttribute_JoinsRolesWithComma()
    {
        var attr = new RoleAuthorizationAttribute("SuperAdmin", "FinanceAdmin", "HRAdmin");

        attr.Roles.Should().Be("SuperAdmin,FinanceAdmin,HRAdmin");
    }

    [Fact]
    public void DepartmentAuthorizationAttribute_StoresDepartmentName_AndDefaultsAllowSuperAdminTrue()
    {
        var attr = new DepartmentAuthorizationAttribute("Finance");

        attr.DepartmentName.Should().Be("Finance");
        attr.AllowSuperAdmin.Should().BeTrue();
    }

    [Fact]
    public void PermissionAuthorizationAttribute_StoresPermissionName()
    {
        var attr = new PermissionAuthorizationAttribute("CanView");

        attr.PermissionName.Should().Be("CanView");
    }

    [Fact]
    public void PageAuthorizationAttribute_StoresPageName()
    {
        var attr = new PageAuthorizationAttribute("Dashboard");

        attr.PageName.Should().Be("Dashboard");
    }
}
