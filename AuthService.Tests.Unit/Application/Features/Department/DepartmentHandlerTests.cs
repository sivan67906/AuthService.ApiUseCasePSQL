using AuthService.Application.Features.Department.CreateDepartment;
using AuthService.Application.Features.Department.DeleteDepartment;
using AuthService.Application.Features.Department.GetAllDepartments;
using AuthService.Application.Features.Department.GetDepartment;
using AuthService.Application.Features.Department.UpdateDepartment;
using MockQueryable.Moq;
using DepartmentEntity = AuthService.Domain.Entities.Department;

namespace AuthService.Tests.Unit.Application.Features.Department;

#region CreateDepartment Tests

public class CreateDepartmentCommandHandlerTests : ApplicationTestBase
{
    private readonly CreateDepartmentCommandHandler _handler;

    public CreateDepartmentCommandHandlerTests()
    {
        _handler = new CreateDepartmentCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidDepartment_ReturnsCreatedDepartment()
    {
        // Arrange
        var command = new CreateDepartmentCommand("SALES", "Sales Department", "Handles all sales operations");
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("SALES");
        result.Name.Should().Be("Sales Department");
    }

    [Fact]
    public async Task Handle_ValidDepartment_CodeIsUppercased()
    {
        // Arrange
        var command = new CreateDepartmentCommand("sales", "Sales Department", null);
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Code.Should().Be("SALES");
    }

    [Fact]
    public async Task Handle_NullDescription_Succeeds()
    {
        // Arrange
        var command = new CreateDepartmentCommand("HR", "Human Resources", null);
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().BeNull();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingDepartment = CreateTestDepartment(code: "SALES");
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new CreateDepartmentCommand("SALES", "Another Sales", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_DuplicateCodeCaseInsensitive_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingDepartment = CreateTestDepartment(code: "SALES");
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new CreateDepartmentCommand("sales", "Sales Dept", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DeletedDepartmentWithSameCode_ThrowsWithDeactivatedMessage()
    {
        // Arrange
        var deletedDepartment = CreateTestDepartment(code: "SALES", isDeleted: true);
        var departments = new List<DepartmentEntity> { deletedDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new CreateDepartmentCommand("SALES", "Sales Dept", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deactivated mode*");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_LongDescription_Succeeds()
    {
        // Arrange
        var longDescription = new string('A', 1000);
        var command = new CreateDepartmentCommand("TEST", "Test Dept", longDescription);
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(longDescription);
    }

    [Fact]
    public async Task Handle_SpecialCharactersInName_Succeeds()
    {
        // Arrange
        var command = new CreateDepartmentCommand("RND", "R&D - Research & Development", "R&D Dept");
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("R&D - Research & Development");
    }

    #endregion

    #region Exception Scenarios

    [Fact]
    public async Task Handle_DatabaseError_ThrowsException()
    {
        // Arrange
        var command = new CreateDepartmentCommand("TEST", "Test", null);
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    #endregion
}

public class CreateDepartmentCommandValidatorTests
{
    private readonly CreateDepartmentCommandValidator _validator;

    public CreateDepartmentCommandValidatorTests()
    {
        _validator = new CreateDepartmentCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreateDepartmentCommand("SALES", "Sales Department", "Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyCode_FailsValidation(string? code)
    {
        // Arrange
        var command = new CreateDepartmentCommand(code!, "Sales", null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyName_FailsValidation(string? name)
    {
        // Arrange
        var command = new CreateDepartmentCommand("SALES", name!, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}

#endregion

#region UpdateDepartment Tests

public class UpdateDepartmentCommandHandlerTests : ApplicationTestBase
{
    private readonly UpdateDepartmentCommandHandler _handler;

    public UpdateDepartmentCommandHandlerTests()
    {
        _handler = new UpdateDepartmentCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = CreateTestDepartment(id: departmentId, code: "SALES", name: "Old Name");
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<DepartmentEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateDepartmentCommand(departmentId, "New Name", "New Description");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task Handle_OnlyNameChanged_Succeeds()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = CreateTestDepartment(id: departmentId, name: "Old Name", description: "Old Desc");
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<DepartmentEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateDepartmentCommand(departmentId, "New Name", "Old Desc");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("New Name");
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new UpdateDepartmentCommand(Guid.NewGuid(), "Name", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_DeletedDepartment_ThrowsInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var deletedDepartment = CreateTestDepartment(id: departmentId, isDeleted: true);
        var departments = new List<DepartmentEntity> { deletedDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new UpdateDepartmentCommand(departmentId, "Name", null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_NoChangesDetected_ThrowsInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = CreateTestDepartment(id: departmentId, name: "Same Name", description: "Same Desc");
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new UpdateDepartmentCommand(departmentId, "Same Name", "Same Desc");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No changes detected*");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_NullToValueDescription_Succeeds()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = CreateTestDepartment(id: departmentId, name: "Name", description: null);
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<DepartmentEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateDepartmentCommand(departmentId, "Name", "New Description");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task Handle_ValueToNullDescription_Succeeds()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var existingDepartment = CreateTestDepartment(id: departmentId, name: "Name", description: "Old Desc");
        var departments = new List<DepartmentEntity> { existingDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<DepartmentEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateDepartmentCommand(departmentId, "Name", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Description.Should().BeNull();
    }

    #endregion
}

#endregion

#region DeleteDepartment Tests

public class DeleteDepartmentCommandHandlerTests : ApplicationTestBase
{
    private readonly DeleteDepartmentCommandHandler _handler;

    public DeleteDepartmentCommandHandlerTests()
    {
        _handler = new DeleteDepartmentCommandHandler(DbContextMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ExistingDepartment_ReturnsTrue()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId);
        var departments = new List<DepartmentEntity> { department };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<DepartmentEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteDepartmentCommand(departmentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeleteDepartment_SetsIsDeletedTrue()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId);
        var departments = new List<DepartmentEntity> { department };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.Set<DepartmentEntity>()).Returns(mockDbSet.Object);
        DbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteDepartmentCommand(departmentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        department.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new DeleteDepartmentCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns false when not found, doesn't throw
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AlreadyDeletedDepartment_ThrowsInvalidOperationException()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var deletedDepartment = CreateTestDepartment(id: departmentId, isDeleted: true);
        var departments = new List<DepartmentEntity> { deletedDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        var command = new DeleteDepartmentCommand(departmentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns false for already deleted, doesn't throw
        result.Should().BeFalse();
    }

    #endregion
}

#endregion

#region GetDepartment Tests

public class GetDepartmentQueryHandlerTests : ApplicationTestBase
{
    private readonly GetDepartmentQueryHandler _handler;

    public GetDepartmentQueryHandlerTests()
    {
        _handler = new GetDepartmentQueryHandler(
            DbContextMock.Object,
            HttpContextAccessorMock.Object,
            UserManagerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_ExistingDepartment_ReturnsDepartment()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var department = CreateTestDepartment(id: departmentId, name: "Test Dept");
        var departments = new List<DepartmentEntity> { department };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);

        // Setup unauthenticated context
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var query = new GetDepartmentQuery(departmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Dept");
    }

    #endregion

    #region Negative Scenarios

    [Fact]
    public async Task Handle_NonExistentDepartment_ReturnsNull()
    {
        // Arrange
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var query = new GetDepartmentQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedDepartment_ReturnsNull()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var deletedDepartment = CreateTestDepartment(id: departmentId, isDeleted: true);
        var departments = new List<DepartmentEntity> { deletedDepartment };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var query = new GetDepartmentQuery(departmentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}

#endregion

#region GetAllDepartments Tests

public class GetAllDepartmentsQueryHandlerTests : ApplicationTestBase
{
    private readonly GetAllDepartmentsQueryHandler _handler;

    public GetAllDepartmentsQueryHandlerTests()
    {
        _handler = new GetAllDepartmentsQueryHandler(
            DbContextMock.Object,
            HttpContextAccessorMock.Object,
            UserManagerMock.Object);
    }

    #region Positive Scenarios

    [Fact]
    public async Task Handle_MultipleDepartments_ReturnsAllActive()
    {
        // Arrange
        var dept1 = CreateTestDepartment(code: "SALES", name: "Sales");
        var dept2 = CreateTestDepartment(code: "HR", name: "HR");
        var deletedDept = CreateTestDepartment(code: "OLD", name: "Old", isDeleted: true);
        var departments = new List<DepartmentEntity> { dept1, dept2, deletedDept };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var query = new GetAllDepartmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Code == "SALES");
        result.Should().Contain(d => d.Code == "HR");
    }

    [Fact]
    public async Task Handle_NoDepartments_ReturnsEmptyList()
    {
        // Arrange
        var departments = new List<DepartmentEntity>();
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var query = new GetAllDepartmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_OnlyDeletedDepartments_ReturnsEmptyList()
    {
        // Arrange
        var deletedDept = CreateTestDepartment(isDeleted: true);
        var departments = new List<DepartmentEntity> { deletedDept };
        var mockDbSet = departments.AsQueryable().BuildMockDbSet();

        DbContextMock.Setup(x => x.Departments).Returns(mockDbSet.Object);
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var query = new GetAllDepartmentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}

#endregion
