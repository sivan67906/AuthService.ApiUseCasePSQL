using AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchy;
using MockQueryable.Moq;
using RoleHierarchyEntity = AuthService.Domain.Entities.RoleHierarchy;

namespace AuthService.Tests.Unit.Application.Features.RoleHierarchy;

public class UpdateRoleHierarchyCommandHandlerTests : ApplicationTestBase
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
    public async Task Handle_WhenHierarchyNotFound_ShouldThrow()
    {
        var hierarchies = new List<RoleHierarchyEntity>();
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);

        var handler = new UpdateRoleHierarchyCommandHandler(DbContextMock.Object);
        var id = Guid.NewGuid();

        var act = () => handler.Handle(new UpdateRoleHierarchyCommand
        {
            Id = id,
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Role hierarchy with ID {id} not found");
    }

    [Fact]
    public async Task Handle_WhenDuplicateMapping_ShouldThrow()
    {
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var hierarchies = new List<RoleHierarchyEntity>
        {
            new() { Id = id, ParentRoleId = Guid.NewGuid(), ChildRoleId = Guid.NewGuid(), Level = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = otherId, ParentRoleId = parentId, ChildRoleId = childId, Level = 1, CreatedAt = DateTime.UtcNow }
        };
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        var roles = new List<ApplicationRole>
        {
            CreateTestRole(id: parentId, name: "Parent"),
            CreateTestRole(id: childId, name: "Child")
        };
        var rolesDbSet = BuildRolesDbSet(roles);

        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);
        DbContextMock.SetupGet(x => x.Roles).Returns(rolesDbSet.Object);

        var handler = new UpdateRoleHierarchyCommandHandler(DbContextMock.Object);

        var act = () => handler.Handle(new UpdateRoleHierarchyCommand
        {
            Id = id,
            ParentRoleId = parentId,
            ChildRoleId = childId,
            Level = 2
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("This role hierarchy mapping already exists");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUpdateAndReturnDto()
    {
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var existing = new RoleHierarchyEntity
        {
            Id = id,
            ParentRoleId = Guid.NewGuid(),
            ChildRoleId = Guid.NewGuid(),
            Level = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var hierarchies = new List<RoleHierarchyEntity> { existing };
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        var roles = new List<ApplicationRole>
        {
            CreateTestRole(id: parentId, name: "Parent"),
            CreateTestRole(id: childId, name: "Child")
        };
        var rolesDbSet = BuildRolesDbSet(roles);

        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);
        DbContextMock.SetupGet(x => x.Roles).Returns(rolesDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateRoleHierarchyCommandHandler(DbContextMock.Object);

        var result = await handler.Handle(new UpdateRoleHierarchyCommand
        {
            Id = id,
            ParentRoleId = parentId,
            ChildRoleId = childId,
            Level = 3
        }, CancellationToken.None);

        result.Id.Should().Be(id);
        result.ParentRoleId.Should().Be(parentId);
        result.ChildRoleId.Should().Be(childId);
        result.Level.Should().Be(3);
        result.ParentRoleName.Should().Be("Parent");
        result.ChildRoleName.Should().Be("Child");

        DbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
