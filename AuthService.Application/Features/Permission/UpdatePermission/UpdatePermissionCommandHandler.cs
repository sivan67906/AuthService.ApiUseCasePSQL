using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Permission.CreatePermission;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.UpdatePermission;
public sealed class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, PermissionDto>
{
    private readonly IAppDbContext _db;
    public UpdatePermissionCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PermissionDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Permissions
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Permission with ID {request.Id} not found");
        }
        
        // Check if any changes were made
        bool hasChanges = false;
        if (entity.Name != request.Name || entity.Description != request.Description)
        {
            hasChanges = true;
        }
        
        if (!hasChanges)
        {
            throw new InvalidOperationException("No changes detected. Please modify the data before updating.");
        }
        
        // Check for duplicate name (case-insensitive) excluding current record and soft-deleted records
        var duplicateExists = await _db.Permissions
            .Where(x => !x.IsDeleted && x.Id != request.Id)
            .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException($"Permission with name '{request.Name}' already exists");
        }
        
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.Permission>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdatePermissionHandler] Saved {savedCount} entities for Permission ID: {request.Id}");
        return entity.Adapt<PermissionDto>();
}
}
