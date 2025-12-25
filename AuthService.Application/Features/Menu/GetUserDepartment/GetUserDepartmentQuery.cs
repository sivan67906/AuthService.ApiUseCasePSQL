namespace AuthService.Application.Features.Menu.GetUserDepartment;

public record GetUserDepartmentQuery(Guid UserId) : IRequest<Guid?>;
