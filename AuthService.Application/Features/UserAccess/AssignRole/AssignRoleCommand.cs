namespace AuthService.Application.Features.UserAccess.AssignRole;

/// <summary>
/// Command to assign a role to a user
/// </summary>
public sealed record AssignRoleCommand(
    string EmailId,
    string RoleName,
    Guid DepartmentId
) : IRequest<bool>;