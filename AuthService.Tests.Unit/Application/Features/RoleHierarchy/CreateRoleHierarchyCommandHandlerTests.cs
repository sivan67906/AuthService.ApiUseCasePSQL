using AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchy;
using MockQueryable.Moq;
using RoleHierarchyEntity = AuthService.Domain.Entities.RoleHierarchy;

namespace AuthService.Tests.Unit.Application.Features.RoleHierarchy;

public class CreateRoleHierarchyCommandHandlerTests : ApplicationTestBase
{
    private static Mock<DbSet<ApplicationRole>> BuildRolesDbSet(List<ApplicationRole> roles)
    {
        var mock = roles.AsQueryable().BuildMockDbSet();
        mock.Setup(d => d.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
            {
                var id = (Guid)keys[0];
                var role = roles.FirstOrDefault(r => r.Id == id);
                return new ValueTask<ApplicationRole?>(role);
            });
        return mock;
    }

    [Fact]
    public async Task Handle_WhenParentRoleMissing_ShouldThrow()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var roles = new List<ApplicationRole> { CreateTestRole(id: childId, name: "Child") };
        var rolesDbSet = BuildRolesDbSet(roles);

        var hierarchies = new List<RoleHierarchyEntity>();
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.Roles).Returns(rolesDbSet.Object);
        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);

        var handler = new CreateRoleHierarchyCommandHandler(DbContextMock.Object);

        var act = () => handler.Handle(new CreateRoleHierarchyCommand
        {
            ParentRoleId = parentId,
            ChildRoleId = childId,
            Level = 1
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Parent role with ID {parentId} not found");
    }

    [Fact]
    public async Task Handle_WhenChildRoleMissing_ShouldThrow()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var roles = new List<ApplicationRole> { CreateTestRole(id: parentId, name: "Parent") };
        var rolesDbSet = BuildRolesDbSet(roles);

        var hierarchies = new List<RoleHierarchyEntity>();
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.Roles).Returns(rolesDbSet.Object);
        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);

        var handler = new CreateRoleHierarchyCommandHandler(DbContextMock.Object);

        var act = () => handler.Handle(new CreateRoleHierarchyCommand
        {
            ParentRoleId = parentId,
            ChildRoleId = childId,
            Level = 1
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Child role with ID {childId} not found");
    }

    [Fact]
    public async Task Handle_WhenMappingAlreadyExists_ShouldThrow()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var roles = new List<ApplicationRole>
        {
            CreateTestRole(id: parentId, name: "Parent"),
            CreateTestRole(id: childId, name: "Child")
        };
        var rolesDbSet = BuildRolesDbSet(roles);

        var hierarchies = new List<RoleHierarchyEntity>
        {
            new() { Id = Guid.NewGuid(), ParentRoleId = parentId, ChildRoleId = childId, Level = 1 }
        };
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.Roles).Returns(rolesDbSet.Object);
        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);

        var handler = new CreateRoleHierarchyCommandHandler(DbContextMock.Object);

        var act = () => handler.Handle(new CreateRoleHierarchyCommand
        {
            ParentRoleId = parentId,
            ChildRoleId = childId,
            Level = 1
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("This role hierarchy mapping already exists");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldAddEntityAndReturnDto()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var roles = new List<ApplicationRole>
        {
            CreateTestRole(id: parentId, name: "Parent"),
            CreateTestRole(id: childId, name: "Child")
        };
        var rolesDbSet = BuildRolesDbSet(roles);

        var hierarchies = new List<RoleHierarchyEntity>();
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.Roles).Returns(rolesDbSet.Object);
        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateRoleHierarchyCommandHandler(DbContextMock.Object);

        var result = await handler.Handle(new CreateRoleHierarchyCommand
        {
            ParentRoleId = parentId,
            ChildRoleId = childId,
            Level = 2
        }, CancellationToken.None);

        result.ParentRoleId.Should().Be(parentId);
        result.ChildRoleId.Should().Be(childId);
        result.Level.Should().Be(2);
        result.ParentRoleName.Should().Be("Parent");
        result.ChildRoleName.Should().Be("Child");

        DbContextMock.Verify(x => x.RoleHierarchies.Add(It.IsAny<RoleHierarchyEntity>()), Times.Once);
        DbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}