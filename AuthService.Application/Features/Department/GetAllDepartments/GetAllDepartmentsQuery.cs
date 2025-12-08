using AuthService.Application.Features.Department.CreateDepartment;

namespace AuthService.Application.Features.Department.GetAllDepartments;
public sealed record GetAllDepartmentsQuery : IRequest<List<DepartmentDto>>;
