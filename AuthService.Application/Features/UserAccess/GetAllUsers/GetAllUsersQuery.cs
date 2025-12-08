namespace AuthService.Application.Features.UserAccess.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<List<UserDto>>;
