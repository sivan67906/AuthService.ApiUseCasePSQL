using AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchy;
using MockQueryable.Moq;
using RoleHierarchyEntity = AuthService.Domain.Entities.RoleHierarchy;

namespace AuthService.Tests.Unit.Application.Features.RoleHierarchy;

public class DeleteRoleHierarchyCommandHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrow()
    {
        var hierarchies = new List<RoleHierarchyEntity>();
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);

        var handler = new DeleteRoleHierarchyCommandHandler(DbContextMock.Object);
        var id = Guid.NewGuid();

        var act = () => handler.Handle(new DeleteRoleHierarchyCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Role hierarchy with ID {id} not found");
    }

    [Fact]
    public async Task Handle_WhenFound_ShouldRemoveAndReturnTrue()
    {
        var id = Guid.NewGuid();
        var entity = new RoleHierarchyEntity { Id = id, ParentRoleId = Guid.NewGuid(), ChildRoleId = Guid.NewGuid(), Level = 1 };
        var hierarchies = new List<RoleHierarchyEntity> { entity };
        var hierarchyDbSet = hierarchies.AsQueryable().BuildMockDbSet();

        DbContextMock.SetupGet(x => x.RoleHierarchies).Returns(hierarchyDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteRoleHierarchyCommandHandler(DbContextMock.Object);

        var result = await handler.Handle(new DeleteRoleHierarchyCommand(id), CancellationToken.None);

        result.Should().BeTrue();
        DbContextMock.Verify(x => x.RoleHierarchies.Remove(It.Is<RoleHierarchyEntity>(rh => rh.Id == id)), Times.Once);
        DbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
