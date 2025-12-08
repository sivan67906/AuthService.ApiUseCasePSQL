using AuthService.Application.Features.Page.CreatePage;

namespace AuthService.Application.Features.Page.UpdatePage;

/// <summary>
/// Command to update an existing page
/// </summary>
public sealed record UpdatePageCommand(
    Guid Id,
    string Name,
    string Url,
    string? Description,
    int DisplayOrder,
    string? MenuContext = null,
    bool IsActive = true
) : IRequest<PageDto>;