using AuthService.Application.Features.Feature.GetFeaturesWithHierarchy;
using MockQueryable.Moq;

using FeatureEntity = AuthService.Domain.Entities.Feature;

namespace AuthService.Tests.Unit.Application.Features.Feature;

public class GetFeaturesWithHierarchyQueryHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task Handle_MainMenuAndSubMenu_BuildsDisplayNameCorrectly()
    {
        // Arrange
        var main = CreateTestFeature(name: "Finance", isMainMenu: true, parentFeatureId: null, displayOrder: 1);
        var sub = CreateTestFeature(name: "Vendors", isMainMenu: false, parentFeatureId: main.Id, displayOrder: 2);
        sub.ParentFeature = main;

        DbContextMock.Setup(x => x.Features)
            .Returns(new List<FeatureEntity> { main, sub }.AsQueryable().BuildMockDbSet().Object);

        var handler = new GetFeaturesWithHierarchyQueryHandler(DbContextMock.Object);

        // Act
        var result = await handler.Handle(new GetFeaturesWithHierarchyQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        var mainDto = result.Single(r => r.Id == main.Id);
        mainDto.DisplayName.Should().Be("Finance (Main Menu)");

        var subDto = result.Single(r => r.Id == sub.Id);
        subDto.DisplayName.Should().Be("Finance → Vendors");
    }
}
