using AuthService.Application.Features.Role.CreateRole;

namespace AuthService.Application.Features.Role.GetRolesByDepartment;

/// <summary>
/// Query to get roles filtered by department
/// If departmentId is null, returns all roles (for "System wide - All Departments")
/// If departmentId is provided, returns only roles for that department
/// </summary>
public sealed record GetRolesByDepartmentQuery(Guid? DepartmentId) : IRequest<List<RoleDto>>;
