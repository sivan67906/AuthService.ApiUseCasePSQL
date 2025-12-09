using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Permission.CreatePermission;
public sealed class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, PermissionDto>
{
    private readonly IAppDbContext _db;
    public CreatePermissionCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PermissionDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name (case-insensitive) - including soft-deleted records
        var existing = await _db.Permissions
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                throw new InvalidOperationException($"A permission with name '{request.Name}' already exists in deactivated mode. Please use a different name.");
            }
            else
            {
                throw new InvalidOperationException($"Permission with name '{request.Name}' already exists");
            }
        }
        
        var entity = new Domain.Entities.Permission
        {
            Name = request.Name,
            Description = request.Description
        };
        _db.Permissions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<PermissionDto>();
}


}
