using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Feature.CreateFeature;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.UpdateFeature;
public sealed class UpdateFeatureCommandHandler : IRequestHandler<UpdateFeatureCommand, FeatureDto>
{
    private readonly IAppDbContext _db;
    public UpdateFeatureCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<FeatureDto> Handle(UpdateFeatureCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Features
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Feature with ID {request.Id} not found");
        }
        
        // Validate DisplayOrder (no negative values)
        if (request.DisplayOrder < 0)
        {
            throw new InvalidOperationException("Display Order cannot be negative");
        }
        
        // Check if any changes were made
        bool hasChanges = false;
        if (entity.Name != request.Name || 
            entity.Description != request.Description ||
            entity.Icon != request.Icon ||
            entity.IsMainMenu != request.IsMainMenu ||
            entity.ParentFeatureId != request.ParentFeatureId ||
            entity.DisplayOrder != request.DisplayOrder ||
            entity.IsActive != request.IsActive)
        {
            hasChanges = true;
        }
        
        if (!hasChanges)
        {
            throw new InvalidOperationException("No changes detected. Please modify the data before updating.");
        }
        
        // Check for duplicate name (case-insensitive) excluding current record and soft-deleted records
        var duplicateExists = await _db.Features
            .Where(x => !x.IsDeleted && x.Id != request.Id)
            .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException($"Feature with name '{request.Name}' already exists");
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
        
        // Update entity properties
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Icon = request.Icon;
        entity.IsMainMenu = request.IsMainMenu;
        entity.ParentFeatureId = request.ParentFeatureId;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.Level = level;
        entity.RouteUrl = request.RouteUrl ?? entity.RouteUrl ?? ""; // Preserve existing or default to empty
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified
        _db.Set<Domain.Entities.Feature>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdateFeatureHandler] Saved {savedCount} entities for Feature ID: {request.Id}");
        
        return entity.Adapt<FeatureDto>();
}
}
