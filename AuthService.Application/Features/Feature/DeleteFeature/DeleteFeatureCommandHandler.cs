using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.DeleteFeature;
public sealed class DeleteFeatureCommandHandler : IRequestHandler<DeleteFeatureCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeleteFeatureCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeleteFeatureCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Features
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            Console.WriteLine($"[DeleteFeatureHandler] Feature not found: {request.Id}");
            return false;
        }
        
        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified
        _db.Set<Domain.Entities.Feature>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeleteFeatureHandler] Saved {savedCount} entities for Feature ID: {request.Id}");
        
        return true;
}
}
