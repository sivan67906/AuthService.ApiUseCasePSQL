using AuthService.Application.Features.RolePagePermissionMapping.CreateOrUpdateBatch;
using AuthService.Application.Features.RolePagePermissionMapping.GetGroupedRolePagePermissions;
using AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingsByDepartment;

using DepartmentEntity = AuthService.Domain.Entities.Department;
using PageEntity = AuthService.Domain.Entities.Page;
using PermissionEntity = AuthService.Domain.Entities.Permission;
using RolePagePermissionMappingEntity = AuthService.Domain.Entities.RolePagePermissionMapping;

namespace AuthService.Tests.Unit.Application.Features.RolePagePermissionMapping;

public class RolePagePermissionAdvancedHandlerTests
{
    private static AppDbContext CreateInMemoryDbContext(string? name = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name ?? $"rppm-db-{Guid.NewGuid():N}")
            .EnableSensitiveDataLogging()
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task GetRolePagePermissionMappingsByDepartment_ReturnsOnlyDepartmentMappings()
    {
        await using var db = CreateInMemoryDbContext();

        var deptA = new DepartmentEntity { Id = Guid.NewGuid(), Code = "D1", Name = "Dept A", CreatedAt = DateTime.UtcNow };
        var deptB = new DepartmentEntity { Id = Guid.NewGuid(), Code = "D2", Name = "Dept B", CreatedAt = DateTime.UtcNow };
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role1", Code = "R1" };
        var page = new PageEntity { Id = Guid.NewGuid(), Code = "P1", Name = "Page 1", Url = "/p1", DisplayOrder = 1, CreatedAt = DateTime.UtcNow };
        var perm = new PermissionEntity { Id = Guid.NewGuid(), Code = "V", Name = "View", CreatedAt = DateTime.UtcNow };

        db.Departments.AddRange(deptA, deptB);
        db.ApplicationRoles.Add(role);
        db.Pages.Add(page);
        db.Permissions.Add(perm);

        db.RolePagePermissionMappings.AddRange(
            new RolePagePermissionMappingEntity { Id = Guid.NewGuid(), DepartmentId = deptA.Id, RoleId = role.Id, PageId = page.Id, PermissionId = perm.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
            new RolePagePermissionMappingEntity { Id = Guid.NewGuid(), DepartmentId = deptB.Id, RoleId = role.Id, PageId = page.Id, PermissionId = perm.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();

        var handler = new GetRolePagePermissionMappingsByDepartmentQueryHandler(db);
        var result = await handler.Handle(new GetRolePagePermissionMappingsByDepartmentQuery(deptA.Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].DepartmentId.Should().Be(deptA.Id);
        result[0].PermissionName.Should().Be("View");
    }

    [Fact]
    public async Task GetGroupedRolePagePermissions_GroupsByDepartmentRolePageAndOrdersPermissions()
    {
        await using var db = CreateInMemoryDbContext();

        var dept = new DepartmentEntity { Id = Guid.NewGuid(), Code = "D1", Name = "Dept A", CreatedAt = DateTime.UtcNow };
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role1", Code = "R1" };
        var page = new PageEntity { Id = Guid.NewGuid(), Code = "P1", Name = "Page 1", Url = "/p1", DisplayOrder = 1, CreatedAt = DateTime.UtcNow };
        var view = new PermissionEntity { Id = Guid.NewGuid(), Code = "V", Name = "View", CreatedAt = DateTime.UtcNow };
        var create = new PermissionEntity { Id = Guid.NewGuid(), Code = "C", Name = "Create", CreatedAt = DateTime.UtcNow };

        db.Departments.Add(dept);
        db.ApplicationRoles.Add(role);
        db.Pages.Add(page);
        db.Permissions.AddRange(view, create);

        db.RolePagePermissionMappings.AddRange(
            new RolePagePermissionMappingEntity { Id = Guid.NewGuid(), DepartmentId = dept.Id, RoleId = role.Id, PageId = page.Id, PermissionId = create.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(2) },
            new RolePagePermissionMappingEntity { Id = Guid.NewGuid(), DepartmentId = dept.Id, RoleId = role.Id, PageId = page.Id, PermissionId = view.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = new GetGroupedRolePagePermissionsQueryHandler(db);
        var groups = await handler.Handle(new GetGroupedRolePagePermissionsQuery(), CancellationToken.None);

        groups.Should().HaveCount(1);
        var g = groups[0];
        g.Permissions.Should().HaveCount(2);
        g.Permissions[0].PermissionName.Should().Be("View");
        g.Permissions[0].PermissionCode.Should().Be("V");
        g.Permissions[1].PermissionName.Should().Be("Create");
        g.Permissions[1].PermissionCode.Should().Be("C");
    }

    [Fact]
    public async Task CreateOrUpdateRolePagePermissionBatch_RequiresViewPermission()
    {
        await using var db = CreateInMemoryDbContext();

        var deptId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        var view = new PermissionEntity { Id = Guid.NewGuid(), Code = "V", Name = "View", CreatedAt = DateTime.UtcNow };
        var edit = new PermissionEntity { Id = Guid.NewGuid(), Code = "E", Name = "Edit", CreatedAt = DateTime.UtcNow };
        db.Permissions.AddRange(view, edit);
        db.Departments.Add(new DepartmentEntity { Id = deptId, Code = "D1", Name = "Dept", CreatedAt = DateTime.UtcNow });
        db.ApplicationRoles.Add(new ApplicationRole { Id = roleId, Code = "R1", Name = "Role" });
        db.Pages.Add(new PageEntity { Id = pageId, Code = "P1", Name = "Page", Url = "/p", DisplayOrder = 1, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var handler = new CreateOrUpdateRolePagePermissionBatchCommandHandler(db);

        var cmdMissingView = new CreateOrUpdateRolePagePermissionBatchCommand(deptId, roleId, pageId, new List<Guid> { edit.Id });

        var act = async () => await handler.Handle(cmdMissingView, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*View permission is mandatory*");
    }

    [Fact]
    public async Task CreateOrUpdateRolePagePermissionBatch_AddsAndRemovesMappings()
    {
        await using var db = CreateInMemoryDbContext();

        var deptId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        var view = new PermissionEntity { Id = Guid.NewGuid(), Code = "V", Name = "View", CreatedAt = DateTime.UtcNow };
        var edit = new PermissionEntity { Id = Guid.NewGuid(), Code = "E", Name = "Edit", CreatedAt = DateTime.UtcNow };
        var create = new PermissionEntity { Id = Guid.NewGuid(), Code = "C", Name = "Create", CreatedAt = DateTime.UtcNow };

        db.Permissions.AddRange(view, edit, create);
        db.Departments.Add(new DepartmentEntity { Id = deptId, Code = "D1", Name = "Dept", CreatedAt = DateTime.UtcNow });
        db.ApplicationRoles.Add(new ApplicationRole { Id = roleId, Code = "R1", Name = "Role" });
        db.Pages.Add(new PageEntity { Id = pageId, Code = "P1", Name = "Page", Url = "/p", DisplayOrder = 1, CreatedAt = DateTime.UtcNow });

        // Existing: View + Edit
        db.RolePagePermissionMappings.AddRange(
            new RolePagePermissionMappingEntity { Id = Guid.NewGuid(), DepartmentId = deptId, RoleId = roleId, PageId = pageId, PermissionId = view.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
            new RolePagePermissionMappingEntity { Id = Guid.NewGuid(), DepartmentId = deptId, RoleId = roleId, PageId = pageId, PermissionId = edit.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = new CreateOrUpdateRolePagePermissionBatchCommandHandler(db);

        // New desired set: View + Create (Edit removed, Create added)
        var cmd = new CreateOrUpdateRolePagePermissionBatchCommand(deptId, roleId, pageId, new List<Guid> { view.Id, create.Id });
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Select(r => r.PermissionName).Should().BeEquivalentTo(new[] { "View", "Create" });

        var dbPermNames = await db.RolePagePermissionMappings
            .Where(m => m.DepartmentId == deptId && m.RoleId == roleId && m.PageId == pageId)
            .Join(db.Permissions, m => m.PermissionId, p => p.Id, (m, p) => p.Name)
            .ToListAsync();

        dbPermNames.Should().BeEquivalentTo(new[] { "View", "Create" });
    }
}
