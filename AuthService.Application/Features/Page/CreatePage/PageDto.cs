namespace AuthService.Application.Features.Page.CreatePage;

/// <summary>
/// Data transfer object for a page
/// </summary>
public sealed record PageDto(
    Guid Id,
    string Name,
    string Url,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);