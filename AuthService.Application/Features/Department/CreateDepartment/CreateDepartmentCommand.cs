namespace AuthService.Application.Features.Department.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Code,
    string Name,
    string? Description
) : IRequest<DepartmentDto>;