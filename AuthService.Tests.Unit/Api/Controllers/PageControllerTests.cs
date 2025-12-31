using AuthService.Application.Features.Page.CreatePage;
using AuthService.Application.Features.Page.DeletePage;
using AuthService.Application.Features.Page.GetAllPages;
using AuthService.Application.Features.Page.GetPage;
using AuthService.Application.Features.Page.UpdatePage;

namespace AuthService.Tests.Unit.Api.Controllers;

public class PageControllerTests : ControllerTestBase
{
    private readonly PageController _controller;

    public PageControllerTests()
    {
        _controller = new PageController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedPage()
    {
        // Arrange
        var command = new CreatePageCommand("DASHBOARD", "Dashboard", "/dashboard", "Main dashboard page", 1, "main", true);
        var expectedResult = new PageDto(
            Guid.NewGuid(),
            "DASHBOARD",
            "Dashboard",
            "/dashboard",
            "Main dashboard page",
            1,
            "main",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<PageDto>(result);
        response!.Data!.Code.Should().Be("DASHBOARD");
        response.Data.Url.Should().Be("/dashboard");
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageCommand("DASHBOARD", "Dashboard", "/dashboard", null, 1, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page with name 'Dashboard' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    [Fact]
    public async Task Create_WithDuplicateUrl_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageCommand("NEW_PAGE", "New Page", "/dashboard", null, 1, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page with URL '/dashboard' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageCommand("DASHBOARD", "", "/dashboard", null, 1, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page name is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyUrl_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreatePageCommand("DASHBOARD", "Dashboard", "", null, 1, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<CreatePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page URL is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var command = new UpdatePageCommand(pageId, "Updated Dashboard", "/updated-dashboard", "Updated description", 2, "main", true);
        var expectedResult = new PageDto(
            pageId,
            "DASHBOARD",
            "Updated Dashboard",
            "/updated-dashboard",
            "Updated description",
            2,
            "main",
            true,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(pageId, command);

        // Assert
        var response = AssertOkResult<PageDto>(result);
        response!.Data!.Name.Should().Be("Updated Dashboard");
        response.Data.Url.Should().Be("/updated-dashboard");
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdatePageCommand(differentId, "Updated Dashboard", "/updated", null, 1, null, true);

        // Act
        var result = await _controller.Update(pageId, command);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentPage_ReturnsBadRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var command = new UpdatePageCommand(pageId, "Updated Dashboard", "/updated", null, 1, null, true);

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page not found"));

        // Act
        var result = await _controller.Update(pageId, command);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    [Fact]
    public async Task Update_SetInactive_ReturnsOkWithUpdatedPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var command = new UpdatePageCommand(pageId, "Dashboard", "/dashboard", null, 1, null, false);
        var expectedResult = new PageDto(
            pageId,
            "DASHBOARD",
            "Dashboard",
            "/dashboard",
            null,
            1,
            null,
            false,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdatePageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(pageId, command);

        // Assert
        var response = AssertOkResult<PageDto>(result);
        response!.Data!.IsActive.Should().BeFalse();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(pageId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page not found"));

        // Act
        var result = await _controller.Delete(pageId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithPageInUse_ReturnsBadRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeletePageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Page is in use and cannot be deleted"));

        // Act
        var result = await _controller.Delete(pageId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ReturnsOkWithPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var expectedResult = new PageDto(
            pageId,
            "DASHBOARD",
            "Dashboard",
            "/dashboard",
            "Main dashboard",
            1,
            "main",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(pageId);

        // Assert
        var response = AssertOkResult<PageDto>(result);
        response!.Data!.Id.Should().Be(pageId);
        response.Data.Url.Should().Be("/dashboard");
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPageQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<PageDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.Get(pageId);

        // Assert
        AssertNotFoundResult<PageDto>(result);
    }

    [Fact]
    public async Task Get_WithException_ReturnsBadRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetPageQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Get(pageId);

        // Assert
        AssertBadRequestResult<PageDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithPages_ReturnsOkWithList()
    {
        // Arrange
        var pages = new List<PageDto>
        {
            new(Guid.NewGuid(), "DASHBOARD", "Dashboard", "/dashboard", null, 1, "main", true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "USERS", "Users", "/users", null, 2, "main", true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "SETTINGS", "Settings", "/settings", null, 3, "main", true, DateTime.UtcNow, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPagesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<PageDto>>(result);
        response!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoPages_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPagesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PageDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<PageDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllPagesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<PageDto>>(result);
    }

    #endregion
}
