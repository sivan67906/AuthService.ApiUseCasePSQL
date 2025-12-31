using AuthService.Application.Features.Feature.CreateFeature;
using AuthService.Application.Features.Feature.DeleteFeature;
using AuthService.Application.Features.Feature.GetAllFeatures;
using AuthService.Application.Features.Feature.GetFeature;
using AuthService.Application.Features.Feature.GetFeaturesWithHierarchy;
using AuthService.Application.Features.Feature.UpdateFeature;

namespace AuthService.Tests.Unit.Api.Controllers;

public class FeatureControllerTests : ControllerTestBase
{
    private readonly FeatureController _controller;

    public FeatureControllerTests()
    {
        _controller = new FeatureController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedFeature()
    {
        // Arrange
        var command = new CreateFeatureCommand("USER_MGMT", "User Management", "Manage users", "/users", true, null, 1, 0, "users-icon", true);
        var expectedResult = new FeatureDto(
            Guid.NewGuid(),
            "USER_MGMT",
            "User Management",
            "Manage users",
            "/users",
            true,
            null,
            null,
            1,
            0,
            "users-icon",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<FeatureDto>(result);
        response!.Data!.Code.Should().Be("USER_MGMT");
        response.Data.Name.Should().Be("User Management");
    }

    [Fact]
    public async Task Create_WithParentFeature_ReturnsOkWithCreatedFeature()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var command = new CreateFeatureCommand("SUB_FEATURE", "Sub Feature", "Sub feature desc", "/sub", false, parentId, 1, 1, null, true);
        var expectedResult = new FeatureDto(
            Guid.NewGuid(),
            "SUB_FEATURE",
            "Sub Feature",
            "Sub feature desc",
            "/sub",
            false,
            parentId,
            "Parent Feature",
            1,
            1,
            null,
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<FeatureDto>(result);
        response!.Data!.ParentFeatureId.Should().Be(parentId);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateFeatureCommand("USER_MGMT", "User Management", null, null, true, null, 1, 0, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Feature with name 'User Management' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<FeatureDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateFeatureCommand("CODE", "", null, null, true, null, 1, 0, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Feature name is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<FeatureDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedFeature()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var command = new UpdateFeatureCommand(featureId, "Updated Feature", "Updated desc", "/updated", true, null, 2, 0, "new-icon", true);
        var expectedResult = new FeatureDto(
            featureId,
            "USER_MGMT",
            "Updated Feature",
            "Updated desc",
            "/updated",
            true,
            null,
            null,
            2,
            0,
            "new-icon",
            true,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(featureId, command);

        // Assert
        var response = AssertOkResult<FeatureDto>(result);
        response!.Data!.Name.Should().Be("Updated Feature");
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdateFeatureCommand(differentId, "Updated Feature", null, null, true, null, 1, 0, null, true);

        // Act
        var result = await _controller.Update(featureId, command);

        // Assert
        AssertBadRequestResult<FeatureDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentFeature_ReturnsBadRequest()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var command = new UpdateFeatureCommand(featureId, "Updated Feature", null, null, true, null, 1, 0, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Feature not found"));

        // Act
        var result = await _controller.Update(featureId, command);

        // Assert
        AssertBadRequestResult<FeatureDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var featureId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(featureId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var featureId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Feature not found"));

        // Act
        var result = await _controller.Delete(featureId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithChildFeatures_ReturnsBadRequest()
    {
        // Arrange
        var featureId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteFeatureCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete feature with child features"));

        // Act
        var result = await _controller.Delete(featureId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ReturnsOkWithFeature()
    {
        // Arrange
        var featureId = Guid.NewGuid();
        var expectedResult = new FeatureDto(
            featureId,
            "USER_MGMT",
            "User Management",
            "Description",
            "/users",
            true,
            null,
            null,
            1,
            0,
            "icon",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<GetFeatureQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(featureId);

        // Assert
        var response = AssertOkResult<FeatureDto>(result);
        response!.Data!.Id.Should().Be(featureId);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var featureId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetFeatureQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<FeatureDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.Get(featureId);

        // Assert
        AssertNotFoundResult<FeatureDto>(result);
    }

    [Fact]
    public async Task Get_WithException_ReturnsBadRequest()
    {
        // Arrange
        var featureId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetFeatureQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Get(featureId);

        // Assert
        AssertBadRequestResult<FeatureDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithFeatures_ReturnsOkWithList()
    {
        // Arrange
        var features = new List<FeatureDto>
        {
            new(Guid.NewGuid(), "USER_MGMT", "User Management", null, "/users", true, null, null, 1, 0, null, true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "SETTINGS", "Settings", null, "/settings", true, null, null, 2, 0, null, true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "REPORTS", "Reports", null, "/reports", true, null, null, 3, 0, null, true, DateTime.UtcNow, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllFeaturesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(features);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<FeatureDto>>(result);
        response!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoFeatures_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllFeaturesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<FeatureDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllFeaturesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<FeatureDto>>(result);
    }

    #endregion

    #region GetWithHierarchy Tests

    [Fact]
    public async Task GetWithHierarchy_WithHierarchicalFeatures_ReturnsOkWithList()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var features = new List<FeatureWithHierarchyDto>
        {
            new(parentId, "Finance Management", null, true, null, null, 1, "finance-icon", true, "/finance", 0, "Finance Management (Main Menu)"),
            new(Guid.NewGuid(), "Test Categories", null, false, parentId, "Finance Management", 1, null, true, "/categories", 1, "Finance Management → Test Categories")
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetFeaturesWithHierarchyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(features);

        // Act
        var result = await _controller.GetWithHierarchy();

        // Assert
        var response = AssertOkResult<List<FeatureWithHierarchyDto>>(result);
        response!.Data.Should().HaveCount(2);
        response.Data![0].DisplayName.Should().Contain("Main Menu");
        response.Data[1].DisplayName.Should().Contain("→");
    }

    [Fact]
    public async Task GetWithHierarchy_WithNoFeatures_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetFeaturesWithHierarchyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureWithHierarchyDto>());

        // Act
        var result = await _controller.GetWithHierarchy();

        // Assert
        var response = AssertOkResult<List<FeatureWithHierarchyDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWithHierarchy_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetFeaturesWithHierarchyQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetWithHierarchy();

        // Assert
        AssertBadRequestResult<List<FeatureWithHierarchyDto>>(result);
    }

    #endregion
}
