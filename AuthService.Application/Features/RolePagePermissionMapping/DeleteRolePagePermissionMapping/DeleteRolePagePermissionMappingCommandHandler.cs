using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.DeleteRolePagePermissionMapping;

public sealed class DeleteRolePagePermissionMappingCommandHandler : IRequestHandler<DeleteRolePagePermissionMappingCommand, bool>
{
    private readonly IAppDbContext _db;

    public DeleteRolePagePermissionMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteRolePagePermissionMappingCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _db.RolePagePermissionMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (mapping == null)
        {
            Console.WriteLine($"[DeleteRolePagePermissionMappingHandler] RolePagePermissionMapping not found: {request.Id}");
            throw new KeyNotFoundException($"Role page permission mapping with ID {request.Id} not found");
        }

        _db.RolePagePermissionMappings.Remove(mapping);
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeleteRolePagePermissionMappingHandler] Saved {savedCount} entities for RolePagePermissionMapping ID: {request.Id}");

        return true;
    }
}
