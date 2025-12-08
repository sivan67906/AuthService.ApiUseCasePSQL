namespace AuthService.Application.Features.Permission.CreatePermission;

public sealed record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);