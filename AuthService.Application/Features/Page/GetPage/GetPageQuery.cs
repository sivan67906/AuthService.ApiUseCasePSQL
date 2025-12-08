using AuthService.Application.Features.Page.CreatePage;

namespace AuthService.Application.Features.Page.GetPage;
public sealed record GetPageQuery(Guid Id) : IRequest<PageDto?>;
