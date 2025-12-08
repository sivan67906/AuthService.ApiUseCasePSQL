using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Department.DeleteDepartment;
public sealed class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeleteDepartmentCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Departments
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            Console.WriteLine($"[DeleteDepartmentHandler] Department not found: {request.Id}");
            return false;
        }
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.Department>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeleteDepartmentHandler] Saved {savedCount} entities for Department ID: {request.Id}");
        return true;
}
}
