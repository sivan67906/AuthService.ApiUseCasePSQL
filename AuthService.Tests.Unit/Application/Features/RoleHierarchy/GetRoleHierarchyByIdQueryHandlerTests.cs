using AuthService.Application.Features.RoleHierarchyMapping.GetRoleHierarchyById;
using MockQueryable.Moq;
using RoleHierarchyEntity = AuthService.Domain.Entities.RoleHierarchy;

namespace AuthService.Tests.Unit.Application.Features.RoleHierarchy;

public class GetRoleHierarchyByIdQueryHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrow()
    {
        // Arrange
        var id = Guid.NewGuid();
        var set = new List<RoleHierarchyEntity>().AsQueryable().BuildMockDbSet();
        DbContextMock.SetupGet(d => d.RoleHierarchies).Returns(set.Object);

        var handler = new GetRoleHierarchyByIdQueryHandler(DbContextMock.Object);

        // Act
        var act = () => handler.Handle(new GetRoleHierarchyByIdQuery(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Role hierarchy with ID {id} not found");
    }

    [Fact]
    public async Task Handle_WhenFound_ShouldReturnDto()
    {
        // Arrange
        var parent = CreateTestRole(name: "Parent");
        var child = CreateTestRole(name: "Child");
        var id = Guid.NewGuid();

        var entity = new RoleHierarchyEntity
        {
            Id = id,
            ParentRoleId = parent.Id,
            ChildRoleId = child.Id,
            ParentRole = parent,
            ChildRole = child,
            Level = 1,
            CreatedAt = DateTime.UtcNow
        };

        var set = new List<RoleHierarchyEntity> { entity }.AsQueryable().BuildMockDbSet();
        DbContextMock.SetupGet(d => d.RoleHierarchies).Returns(set.Object);

        var handler = new GetRoleHierarchyByIdQueryHandler(DbContextMock.Object);

        // Act
        var result = await handler.Handle(new GetRoleHierarchyByIdQuery(id), CancellationToken.None);

        // Assert
        result.Id.Should().Be(id);
        result.ParentRoleName.Should().Be("Parent");
        result.ChildRoleName.Should().Be("Child");
    }
}
