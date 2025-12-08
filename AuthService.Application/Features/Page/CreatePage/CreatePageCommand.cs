namespace AuthService.Application.Features.Page.CreatePage;

/// <summary>
/// Command to create a new page
/// </summary>
public sealed record CreatePageCommand(
    string Name,
    string Url,
    string? Description,
    int DisplayOrder,
    string? MenuContext = null,
    bool IsActive = true
) : IRequest<PageDto>;