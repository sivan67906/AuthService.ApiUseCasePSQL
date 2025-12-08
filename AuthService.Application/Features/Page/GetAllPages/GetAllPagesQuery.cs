using AuthService.Application.Features.Page.CreatePage;

namespace AuthService.Application.Features.Page.GetAllPages;
public sealed record GetAllPagesQuery : IRequest<List<PageDto>>;
