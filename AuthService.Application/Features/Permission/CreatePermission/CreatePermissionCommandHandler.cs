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
        // Check for duplicate code (case-insensitive) - including soft-deleted records
        var existingByCode = await _db.Permissions
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);
            
        if (existingByCode != null)
        {
            if (existingByCode.IsDeleted)
            {
                throw new InvalidOperationException($"A permission with code '{request.Code}' already exists in deactivated mode. Please use a different code.");
            }
            else
            {
                throw new InvalidOperationException($"Permission with code '{request.Code}' already exists");
            }
        }
        
        var entity = new Domain.Entities.Permission
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description
        };
        _db.Permissions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<PermissionDto>();
}


}
