using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.CreateFeature;

public sealed class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, FeatureDto>
{
    private readonly IAppDbContext _db;
    public CreateFeatureCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<FeatureDto> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
    {
        // Validate DisplayOrder (no negative values)
        if (request.DisplayOrder < 0)
        {
            throw new InvalidOperationException("Display Order cannot be negative");
        }

        // Check for duplicate code (case-insensitive) - including soft-deleted records
        var existingByCode = await _db.Features
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);

        if (existingByCode != null)
        {
            if (existingByCode.IsDeleted)
            {
                throw new InvalidOperationException($"A feature with code '{request.Code}' already exists in deactivated mode. Please use a different code.");
            }
            else
            {
                throw new InvalidOperationException($"Feature with code '{request.Code}' already exists");
            }
        }

        // Automatically set Level based on ParentFeatureId
        int level = 0;
        if (request.ParentFeatureId.HasValue)
        {
            // If parent feature is selected, set Level = 1
            level = 1;

            // Validate that parent exists
            var parentExists = await _db.Features
                .AnyAsync(f => f.Id == request.ParentFeatureId.Value && !f.IsDeleted, cancellationToken);
            if (!parentExists)
            {
                throw new InvalidOperationException("Parent feature not found");
            }
        }
        else
        {
            // If no parent (No Parent), Level = 0 and IsMainMenu must be true
            level = 0;
            if (!request.IsMainMenu)
            {
                throw new InvalidOperationException("Main features (without parent) must have IsMainMenu checked");
            }
        }

        var entity = new Domain.Entities.Feature
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            IsMainMenu = request.IsMainMenu,
            ParentFeatureId = request.ParentFeatureId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            Level = level,
            RouteUrl = request.RouteUrl ?? "" // Default to empty string if not provided
        };
        _db.Features.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<FeatureDto>();
    }


}