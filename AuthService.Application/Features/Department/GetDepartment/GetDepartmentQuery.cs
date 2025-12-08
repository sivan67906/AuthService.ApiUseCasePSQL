using AuthService.Application.Features.Department.CreateDepartment;

namespace AuthService.Application.Features.Department.GetDepartment;
public sealed record GetDepartmentQuery(Guid Id) : IRequest<DepartmentDto?>;
