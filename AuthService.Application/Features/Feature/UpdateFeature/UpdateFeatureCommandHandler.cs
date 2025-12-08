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
        
        // Check for duplicate name
        var exists = await _db.Features
            .AnyAsync(x => x.Name == request.Name && x.Id != request.Id && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Feature with name '{request.Name}' already exists");
        }
        
        // Update entity properties
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Icon = request.Icon;
        entity.IsMainMenu = request.IsMainMenu;
        entity.ParentFeatureId = request.ParentFeatureId;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified
        _db.Set<Domain.Entities.Feature>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdateFeatureHandler] Saved {savedCount} entities for Feature ID: {request.Id}");
        
        return entity.Adapt<FeatureDto>();
}
}
