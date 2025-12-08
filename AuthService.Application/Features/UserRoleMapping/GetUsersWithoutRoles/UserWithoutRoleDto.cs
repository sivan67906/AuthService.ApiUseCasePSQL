namespace AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;

public sealed record UserWithoutRoleDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool IsActive { get; init; }
}
