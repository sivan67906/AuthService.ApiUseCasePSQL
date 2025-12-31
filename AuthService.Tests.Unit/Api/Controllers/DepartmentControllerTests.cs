using AuthService.Application.Features.Department.CreateDepartment;
using AuthService.Application.Features.Department.DeleteDepartment;
using AuthService.Application.Features.Department.GetAllDepartments;
using AuthService.Application.Features.Department.GetDepartment;
using AuthService.Application.Features.Department.UpdateDepartment;

namespace AuthService.Tests.Unit.Api.Controllers;

public class DepartmentControllerTests : ControllerTestBase
{
    private readonly DepartmentController _controller;

    public DepartmentControllerTests()
    {
        _controller = new DepartmentController(MediatorMock.Object);
    }

    #region Create Tests

    [Fact]
    public async Task Create_WithValidCommand_ReturnsOkWithCreatedDepartment()
    {
        // Arrange
        var command = new CreateDepartmentCommand("IT", "Information Technology", "IT Department");
        var expectedResult = new DepartmentDto(
            Guid.NewGuid(),
            "IT",
            "Information Technology",
            "IT Department",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertOkResult<DepartmentDto>(result);
        response!.Data!.Code.Should().Be("IT");
        response.Data.Name.Should().Be("Information Technology");
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateDepartmentCommand("IT", "Information Technology", "IT Department");

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Department with this name already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        var response = AssertBadRequestResult<DepartmentDto>(result);
        response!.Message.Should().Contain("Department with this name already exists");
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateDepartmentCommand("IT", "New Department", "Description");

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Department with code 'IT' already exists"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<DepartmentDto>(result);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateDepartmentCommand("IT", "", "Description");

        MediatorMock.Setup(m => m.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Department name is required"));

        // Act
        var result = await _controller.Create(command);

        // Assert
        AssertBadRequestResult<DepartmentDto>(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidCommand_ReturnsOkWithUpdatedDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var command = new UpdateDepartmentCommand(departmentId, "Updated IT", "Updated Description");
        var expectedResult = new DepartmentDto(
            departmentId,
            "IT",
            "Updated IT",
            "Updated Description",
            true,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Update(departmentId, command);

        // Assert
        var response = AssertOkResult<DepartmentDto>(result);
        response!.Data!.Name.Should().Be("Updated IT");
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var command = new UpdateDepartmentCommand(differentId, "Updated IT", "Updated Description");

        // Act
        var result = await _controller.Update(departmentId, command);

        // Assert
        AssertBadRequestResult<DepartmentDto>(result);
    }

    [Fact]
    public async Task Update_WithNonExistentDepartment_ReturnsBadRequest()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var command = new UpdateDepartmentCommand(departmentId, "Updated IT", "Updated Description");

        MediatorMock.Setup(m => m.Send(It.IsAny<UpdateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Department not found"));

        // Act
        var result = await _controller.Update(departmentId, command);

        // Assert
        AssertBadRequestResult<DepartmentDto>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsOk()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(departmentId);

        // Assert
        AssertOkResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Department not found"));

        // Act
        var result = await _controller.Delete(departmentId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    [Fact]
    public async Task Delete_WithDepartmentHavingUsers_ReturnsBadRequest()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<DeleteDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cannot delete department with assigned users"));

        // Act
        var result = await _controller.Delete(departmentId);

        // Assert
        AssertBadRequestResult<bool>(result);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidId_ReturnsOkWithDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var expectedResult = new DepartmentDto(
            departmentId,
            "IT",
            "Information Technology",
            "IT Department",
            true,
            DateTime.UtcNow,
            null
        );

        MediatorMock.Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Get(departmentId);

        // Assert
        var response = AssertOkResult<DepartmentDto>(result);
        response!.Data!.Id.Should().Be(departmentId);
    }

    [Fact]
    public async Task Get_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
#pragma warning disable CS8620
            .Returns(Task.FromResult<DepartmentDto?>(null));
#pragma warning restore CS8620

        // Act
        var result = await _controller.Get(departmentId);

        // Assert
        AssertNotFoundResult<DepartmentDto>(result);
    }

    [Fact]
    public async Task Get_WithException_ReturnsBadRequest()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        MediatorMock.Setup(m => m.Send(It.IsAny<GetDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Get(departmentId);

        // Assert
        AssertBadRequestResult<DepartmentDto>(result);
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithDepartments_ReturnsOkWithList()
    {
        // Arrange
        var departments = new List<DepartmentDto>
        {
            new(Guid.NewGuid(), "IT", "Information Technology", "IT Dept", true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "HR", "Human Resources", "HR Dept", true, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "FIN", "Finance", "Finance Dept", true, DateTime.UtcNow, null)
        };

        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllDepartmentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<DepartmentDto>>(result);
        response!.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithNoDepartments_ReturnsOkWithEmptyList()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllDepartmentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepartmentDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var response = AssertOkResult<List<DepartmentDto>>(result);
        response!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithException_ReturnsBadRequest()
    {
        // Arrange
        MediatorMock.Setup(m => m.Send(It.IsAny<GetAllDepartmentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        AssertBadRequestResult<List<DepartmentDto>>(result);
    }

    #endregion
}
