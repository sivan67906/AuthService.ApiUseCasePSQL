using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.UpdatePageFeatureMapping;
public sealed class UpdatePageFeatureMappingCommandHandler : IRequestHandler<UpdatePageFeatureMappingCommand, PageFeatureMappingDto>
{
    private readonly IAppDbContext _db;
    public UpdatePageFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PageFeatureMappingDto> Handle(UpdatePageFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.PageFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"PageFeatureMapping with ID {request.Id} not found");
        }
        // Check if new mapping already exists (if changing)
        if (entity.PageId != request.PageId || entity.FeatureId != request.FeatureId)
        {
            var exists = await _db.PageFeatureMappings
                .AnyAsync(x => x.PageId == request.PageId && x.FeatureId == request.FeatureId && x.Id != request.Id, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("This page-feature mapping already exists");
            }
        }
        entity.PageId = request.PageId;
        entity.FeatureId = request.FeatureId;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.PageFeatureMapping>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdatePageFeatureMappingHandler] Saved {savedCount} entities for PageFeatureMapping ID: {request.Id}");
        return entity.Adapt<PageFeatureMappingDto>();
    }
}
