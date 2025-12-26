using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.GetFeaturesWithHierarchy;

public sealed class GetFeaturesWithHierarchyQueryHandler
    : IRequestHandler<GetFeaturesWithHierarchyQuery, List<FeatureWithHierarchyDto>>
{
    private readonly IAppDbContext _db;

    public GetFeaturesWithHierarchyQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<FeatureWithHierarchyDto>> Handle(
        GetFeaturesWithHierarchyQuery request,
        CancellationToken cancellationToken)
    {
        var features = await _db.Features
            .AsNoTracking()
            .Include(f => f.ParentFeature)
            .Where(f => !f.IsDeleted && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(cancellationToken);

        return features.Select(f => new FeatureWithHierarchyDto(
            f.Id,
            f.Name,
            f.Description,
            f.IsMainMenu,
            f.ParentFeatureId,
            f.ParentFeature?.Name,
            f.DisplayOrder,
            f.Icon,
            f.IsActive,
            f.RouteUrl,
            f.Level,
            BuildDisplayName(f.Name, f.ParentFeature?.Name, f.ParentFeatureId)
        )).ToList();
    }

    private static string BuildDisplayName(string featureName, string? parentName, Guid? parentId)
    {
        // Main Menu: "Finance Management (Main Menu)"
        if (!parentId.HasValue)
        {
            return $"{featureName} (Main Menu)";
        }

        // SubMenu: "Finance Management → Test Categories"
        return $"{parentName} → {featureName}";
    }
}
