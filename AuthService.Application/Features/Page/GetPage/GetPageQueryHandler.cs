using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Page.CreatePage;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.GetPage;
public sealed class GetPageQueryHandler : IRequestHandler<GetPageQuery, PageDto?>
{
    private readonly IAppDbContext _db;
    public GetPageQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PageDto?> Handle(GetPageQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Pages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        return entity?.Adapt<PageDto>();
}
}
