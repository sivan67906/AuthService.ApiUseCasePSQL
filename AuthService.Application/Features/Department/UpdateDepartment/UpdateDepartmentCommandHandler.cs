using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Department.CreateDepartment;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Department.UpdateDepartment;
public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IAppDbContext _db;
    public UpdateDepartmentCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Departments
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"Department with ID {request.Id} not found");
        }
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified to ensure EF tracks the changes
        _db.Set<Domain.Entities.Department>().Update(entity);
        
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[UpdateDepartmentHandler] Saved {savedCount} entities for Department ID: {request.Id}");
        return entity.Adapt<DepartmentDto>();
}
}
