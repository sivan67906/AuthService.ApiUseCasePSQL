using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.DeletePageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.GetAllPageFeatureMappings;
using AuthService.Application.Features.PageFeatureMapping.GetPageFeatureMapping;
using AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;

namespace AuthService.Tests.Unit.Api.Controllers;

public class PageFeatureMappingControllerTests : ControllerTestBase
{
    private readonly PageFeatureMappingController _controller;

    public PageFeatureMappingControllerTests()
    {
        _controller = new PageFeatureMappingController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedMapping()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var command = new CreatePageFeatureMappingCommand(pageId, featureId);
        var expectedResult = new PageFeatureMappingDto(
            Guid.NewGuid(),
            pageId,
            featureId,
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<PageFeatureMappingDto>(result);
        response!.Data!.PageId.Should().Be(pageId);
        response.Data.FeatureId.Should().Be(featureId);
    }

    [Fact]
    public async Task Create_WithDuplicateMapping_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid());

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mapping already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithNonExistentPage_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid());

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page not found"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithNonExistentFeature_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid());

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Feature not found"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Create_WithException_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageFeatureMappingCommand(Guid.NewGuid(), Guid.NewGuid());

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var command = new UpdatePageFeatureMappingCommand(mappingId, pageId, featureId);
        var expectedResult = new PageFeatureMappingDto(
            mappingId,
            pageId,
            featureId,
            true,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        var response = AssertOkResult<PageFeatureMappingDto>(result);
        response!.Data!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdatePageFeatureMappingCommand(differentId, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentMapping_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var command = new UpdatePageFeatureMappingCommand(mappingId, Guid.NewGuid(), Guid.NewGuid());

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mapping not found"));

        // Act
        var result = await _controller.Update(mappingId, command);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePageFeatureMappingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mapping not found"));

        // Act
        var result = await _controller.Delete(mappingId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ReturnsOkWithMapping()
    {
        // Arrange
        var mappingId = Guid.NewGuid();
        var expectedResult = new PageFeatureMappingDto(
            mappingId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPageFeatureMappingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(mappingId);

        // Assert
        var response = AssertOkResult<PageFeatureMappingDto>(result);
        response!.Data!.Id.Should().Be(mappingId);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPageFeatureMappingQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<PageFeatureMappingDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.Get(mappingId);

        // Assert
        AssertNotFoundResult<PageFeatureMappingDto>(result);
    }

    [Fact]
    public async Task Get_WithException_ReturnsBadRequest()
    {
        // Arrange
        var mappingId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPageFeatureMappingQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Get(mappingId);

        // Assert
        AssertBadRequestResult<PageFeatureMappingDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithMappings_ReturnsOkWithList()
    {
        // Arrange
        var mappings = new List<PageFeatureMappingDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true, DateTime.UtcNow, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPageFeatureMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappings);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<PageFeatureMappingDto>>(result);
        response!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoMappings_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPageFeatureMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PageFeatureMappingDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<PageFeatureMappingDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPageFeatureMappingsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<PageFeatureMappingDto>>(result);
    }

    #endregion
}
