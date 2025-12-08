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
        // Check for duplicate name
        var exists = await _db.Permissions
            .AnyAsync(x => x.Name == request.Name && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Permission with name '{request.Name}' already exists");
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