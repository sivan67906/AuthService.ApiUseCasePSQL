using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.GetPageFeatureMapping;
public sealed class GetPageFeatureMappingQueryHandler : IRequestHandler<GetPageFeatureMappingQuery, PageFeatureMappingDto?>
{
    private readonly IAppDbContext _db;
    public GetPageFeatureMappingQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PageFeatureMappingDto?> Handle(GetPageFeatureMappingQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.PageFeatureMappings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        return entity?.Adapt<PageFeatureMappingDto>();
}
}
