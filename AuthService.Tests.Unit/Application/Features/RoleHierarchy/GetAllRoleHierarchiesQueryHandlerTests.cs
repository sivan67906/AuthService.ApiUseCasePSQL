using AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchies;
using MockQueryable.Moq;
using RoleHierarchyEntity = AuthService.Domain.Entities.RoleHierarchy;

namespace AuthService.Tests.Unit.Application.Features.RoleHierarchy;

public class GetAllRoleHierarchiesQueryHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task Handle_ShouldReturnProjectedDtos()
    {
        // Arrange
        var parent = CreateTestRole(name: "Parent");
        var child = CreateTestRole(name: "Child");

        var entity = new RoleHierarchyEntity
        {
            Id = Guid.NewGuid(),
            ParentRoleId = parent.Id,
            ChildRoleId = child.Id,
            ParentRole = parent,
            ChildRole = child,
            Level = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var set = new List<RoleHierarchyEntity> { entity }.AsQueryable().BuildMockDbSet();
        DbContextMock.SetupGet(d => d.RoleHierarchies).Returns(set.Object);

        var handler = new GetAllRoleHierarchiesQueryHandler(DbContextMock.Object);

        // Act
        var result = await handler.Handle(new GetAllRoleHierarchiesQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(entity.Id);
        result[0].ParentRoleName.Should().Be("Parent");
        result[0].ChildRoleName.Should().Be("Child");
        result[0].Level.Should().Be(3);
    }
}
