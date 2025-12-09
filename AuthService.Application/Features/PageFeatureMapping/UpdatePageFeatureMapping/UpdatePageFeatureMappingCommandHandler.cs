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
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Page-Feature mapping with ID {request.Id} not found");
        }
        
        // Check if any changes were made
        bool hasChanges = false;
        if (entity.PageId != request.PageId || entity.FeatureId != request.FeatureId)
        {
            hasChanges = true;
        }
        
        if (!hasChanges)
        {
            throw new InvalidOperationException("No changes detected. Please modify the data before updating.");
        }
        
        // Check for duplicate mapping (excluding current and soft-deleted)
        var duplicateExists = await _db.PageFeatureMappings
            .Where(x => !x.IsDeleted && x.Id != request.Id)
            .AnyAsync(x => x.PageId == request.PageId && x.FeatureId == request.FeatureId, cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException("This page-feature mapping already exists");
        }
        
        entity.PageId = request.PageId;
        entity.FeatureId = request.FeatureId;
        entity.UpdatedAt = DateTime.UtcNow;
        
        _db.Set<Domain.Entities.PageFeatureMapping>().Update(entity);
        
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<PageFeatureMappingDto>();
    }
}
