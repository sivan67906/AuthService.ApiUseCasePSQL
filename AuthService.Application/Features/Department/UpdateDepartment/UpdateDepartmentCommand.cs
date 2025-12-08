using AuthService.Application.Features.Department.CreateDepartment;

namespace AuthService.Application.Features.Department.UpdateDepartment;

public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<DepartmentDto>;