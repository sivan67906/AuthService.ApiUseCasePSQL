using AuthService.Application.Features.UserAccess.GetUserAccess;

namespace AuthService.Application.Features.UserAccess.GetUserAccessByEmail;
public sealed record GetUserAccessByEmailQuery(string Email) : IRequest<UserAccessDto>;
