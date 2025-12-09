using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Page.CreatePage;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.UpdatePage;
public sealed class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto>
{
    private readonly IAppDbContext _db;
    public UpdatePageCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Pages
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Page with ID {request.Id} not found");
        }
        
        // Check if any changes were made
        bool hasChanges = false;
        if (entity.Name != request.Name || 
            entity.Url != request.Url ||
            entity.Description != request.Description ||
            entity.DisplayOrder != request.DisplayOrder ||
            entity.MenuContext != request.MenuContext ||
            entity.IsActive != request.IsActive)
        {
            hasChanges = true;
        }
        
        if (!hasChanges)
        {
            throw new InvalidOperationException("No changes detected. Please modify the data before updating.");
        }
        
        // Check for duplicate name (case-insensitive) excluding current record and soft-deleted records
        var duplicateExists = await _db.Pages
            .Where(x => !x.IsDeleted && x.Id != request.Id)
            .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException($"Page with name '{request.Name}' already exists");
        }
        
        // Update entity properties
        entity.Name = request.Name;
        entity.Url = request.Url;
        entity.Description = request.Description;
        entity.DisplayOrder = request.DisplayOrder;
        entity.MenuContext = request.MenuContext;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.Page>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdatePageHandler] Saved {savedCount} entities for Page ID: {request.Id}");
        
        return entity.Adapt<PageDto>();
}
}
