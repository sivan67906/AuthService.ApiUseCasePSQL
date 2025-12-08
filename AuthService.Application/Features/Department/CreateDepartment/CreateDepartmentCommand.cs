namespace AuthService.Application.Features.Department.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Name,
    string? Description
) : IRequest<DepartmentDto>;