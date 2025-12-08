using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Feature.CreateFeature;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.GetFeature;
public sealed class GetFeatureQueryHandler : IRequestHandler<GetFeatureQuery, FeatureDto?>
{
    private readonly IAppDbContext _db;
    public GetFeatureQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<FeatureDto?> Handle(GetFeatureQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Features.AsNoTracking()
            .Include(x => x.ParentFeature)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            return null;
        }
        return new FeatureDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.RouteUrl,
            entity.IsMainMenu,
            entity.ParentFeatureId,
            entity.ParentFeature?.Name,
            entity.DisplayOrder,
            entity.Level,
            entity.Icon,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
}
}
