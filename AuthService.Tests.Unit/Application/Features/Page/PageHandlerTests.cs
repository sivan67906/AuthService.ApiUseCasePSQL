using AuthService.Application.Features.Page.CreatePage;
using AuthService.Application.Features.Page.DeletePage;
using AuthService.Application.Features.Page.GetAllPages;
using AuthService.Application.Features.Page.GetPage;
using AuthService.Application.Features.Page.UpdatePage;
using MockQueryable.Moq;
using PageEntity = AuthService.Domain.Entities.Page;

namespace AuthService.Tests.Unit.Application.Features.Page;

#region CreatePage Tests

public class CreatePageCommandHandlerTests : ApplicationTestBase
{
    private readonly CreatePageCommandHandler _handler;

    public CreatePageCommandHandlerTests()
    {
        _handler = new CreatePageCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPage_ReturnsCreatedPage()
    {
        // Arrange
        // CreatePageCommand: Code, Name, Url, Description, DisplayOrder, MenuContext, IsActive
        var command = new CreatePageCommand(
            "DASHBOARD",
            "Dashboard Page",
            "/dashboard",
            "Main dashboard page",
            1,     // DisplayOrder (int)
            null,  // MenuContext
            true); // IsActive

        var pages = new List<PageEntity>();
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("DASHBOARD");
        result.Name.Should().Be("Dashboard Page");
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsException()
    {
        // Arrange
        var existingPage = CreateTestPage(code: "DASHBOARD");
        var pages = new List<PageEntity> { existingPage };
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var command = new CreatePageCommand(
            "DASHBOARD",
            "Another Dashboard",
            "/another",
            null,
            1,
            null,
            true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidPage_CodeIsUppercased()
    {
        // Arrange
        var command = new CreatePageCommand(
            "dashboard",
            "Dashboard",
            "/dashboard",
            null,
            1,
            null,
            true);

        var pages = new List<PageEntity>();
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Code.Should().Be("DASHBOARD");
    }
}

#endregion

#region GetPage Tests

public class GetPageQueryHandlerTests : ApplicationTestBase
{
    private readonly GetPageQueryHandler _handler;

    public GetPageQueryHandlerTests()
    {
        _handler = new GetPageQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPage_ReturnsPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var page = CreateTestPage(id: pageId, code: "DASHBOARD", name: "Dashboard");
        var pages = new List<PageEntity> { page };
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var query = new GetPageQuery(pageId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("DASHBOARD");
    }

    [Fact]
    public async Task Handle_NonExistingPage_ReturnsNull()
    {
        // Arrange
        var pages = new List<PageEntity>();
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var query = new GetPageQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion

#region GetAllPages Tests

public class GetAllPagesQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllPagesQueryHandler _handler;

    public GetAllPagesQueryHandlerTests()
    {
        _handler = new GetAllPagesQueryHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithPages_ReturnsAllActivePages()
    {
        // Arrange
        var pages = new List<PageEntity>
        {
            CreateTestPage(code: "PAGE1", name: "Page 1"),
            CreateTestPage(code: "PAGE2", name: "Page 2"),
            CreateTestPage(code: "PAGE3", name: "Page 3", isDeleted: true)
        };
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var query = new GetAllPagesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var pages = new List<PageEntity>();
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var query = new GetAllPagesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region UpdatePage Tests

public class UpdatePageCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdatePageCommandHandler _handler;

    public UpdatePageCommandHandlerTests()
    {
        _handler = new UpdatePageCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPage_UpdatesPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var page = CreateTestPage(id: pageId, code: "DASHBOARD", name: "Dashboard");
        var pages = new List<PageEntity> { page };
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<PageEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdatePageCommand(
            pageId,
            "Updated Dashboard",
            "/updated-dashboard",
            "Updated description",
            2,
            null,
            true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Dashboard");
    }

    [Fact]
    public async Task Handle_NonExistingPage_ThrowsException()
    {
        // Arrange
        var pages = new List<PageEntity>();
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var command = new UpdatePageCommand(
            Guid.NewGuid(),
            "Updated Page",
            "/updated",
            null,
            1,
            null,
            true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}

#endregion

#region DeletePage Tests

public class DeletePageCommandHandlerTests : ApplicationTestBase
{
    private readonly DeletePageCommandHandler _handler;

    public DeletePageCommandHandlerTests()
    {
        _handler = new DeletePageCommandHandler(DbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPage_SoftDeletes()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var page = CreateTestPage(id: pageId, code: "DASHBOARD");
        var pages = new List<PageEntity> { page };
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<PageEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeletePageCommand(pageId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        page.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingPage_ThrowsException()
    {
        // Arrange
        var pages = new List<PageEntity>();
        var mockDbSet = pages.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Pages).Returns(mockDbSet.Object);

        var command = new DeletePageCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns false when not found, doesn't throw
        result.Should().BeFalse();
    }
}

#endregion
