namespace AuthService.Application.Features.Department.DeleteDepartment;

public sealed record DeleteDepartmentCommand(Guid Id) : IRequest<bool>;
