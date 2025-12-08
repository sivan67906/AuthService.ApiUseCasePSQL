namespace AuthService.Application.Features.Department.CreateDepartment;

/// <summary>
/// Data transfer object for a department
/// </summary>
public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);