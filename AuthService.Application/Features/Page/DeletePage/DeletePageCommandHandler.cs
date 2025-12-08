using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.DeletePage;
public sealed class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeletePageCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Pages
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            Console.WriteLine($"[DeletePageHandler] Page not found: {request.Id}");
            return false;
        }
        
        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.Page>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeletePageHandler] Saved {savedCount} entities for Page ID: {request.Id}");
        
        return true;
}
}
