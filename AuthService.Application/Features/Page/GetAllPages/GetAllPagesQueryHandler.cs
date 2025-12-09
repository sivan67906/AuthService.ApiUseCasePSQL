using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Page.CreatePage;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.GetAllPages;
public sealed class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, List<PageDto>>
{
    private readonly IAppDbContext _db;
    public GetAllPagesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<PageDto>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.Pages.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Adapt<List<PageDto>>();
    }
}
