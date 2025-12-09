using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Feature.CreateFeature;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.GetAllFeatures;
public sealed class GetAllFeaturesQueryHandler : IRequestHandler<GetAllFeaturesQuery, List<FeatureDto>>
{
    private readonly IAppDbContext _db;
    public GetAllFeaturesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<FeatureDto>> Handle(GetAllFeaturesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.Features.AsNoTracking()
            .Include(x => x.ParentFeature)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Select(x => new FeatureDto(
            x.Id,
            x.Name,
            x.Description,
            x.RouteUrl,
            x.IsMainMenu,
            x.ParentFeatureId,
            x.ParentFeature?.Name,
            x.DisplayOrder,
            x.Level,
            x.Icon,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt
        )).ToList();
}
}
