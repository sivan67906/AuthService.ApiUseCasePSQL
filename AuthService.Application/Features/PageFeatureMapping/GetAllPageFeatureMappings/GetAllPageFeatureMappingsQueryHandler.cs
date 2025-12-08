using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.GetAllPageFeatureMappings;
public sealed class GetAllPageFeatureMappingsQueryHandler : IRequestHandler<GetAllPageFeatureMappingsQuery, List<PageFeatureMappingDto>>
{
    private readonly IAppDbContext _db;
    public GetAllPageFeatureMappingsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<PageFeatureMappingDto>> Handle(GetAllPageFeatureMappingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.PageFeatureMappings.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Adapt<List<PageFeatureMappingDto>>();
}
}
