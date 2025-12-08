using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.DeletePermission;
public sealed class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeletePermissionCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Permissions
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            Console.WriteLine($"[DeletePermissionHandler] Permission not found: {request.Id}");
            return false;
        }
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.Permission>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeletePermissionHandler] Saved {savedCount} entities for Permission ID: {request.Id}");
        return true;
}
}
