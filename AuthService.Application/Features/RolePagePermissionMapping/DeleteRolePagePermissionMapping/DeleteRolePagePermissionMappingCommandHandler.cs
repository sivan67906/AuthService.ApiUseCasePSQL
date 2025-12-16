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
            .Include(m => m.Permission)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (mapping == null)
        {
            Console.WriteLine($"[DeleteRolePagePermissionMappingHandler] RolePagePermissionMapping not found: {request.Id}");
            throw new KeyNotFoundException($"Role page permission mapping with ID {request.Id} not found");
        }

        // Check if this is a View permission
        if (mapping.Permission.Name.Equals("View", StringComparison.OrdinalIgnoreCase))
        {
            // Check if there are other permissions for this combination
            var otherPermissions = await _db.RolePagePermissionMappings
                .Include(m => m.Permission)
                .Where(m => !m.IsDeleted
                         && m.DepartmentId == mapping.DepartmentId
                         && m.RoleId == mapping.RoleId
                         && m.PageId == mapping.PageId
                         && m.Id != request.Id)
                .ToListAsync(cancellationToken);

            if (otherPermissions.Any())
            {
                var otherPermissionNames = string.Join(", ", otherPermissions.Select(p => p.Permission.Name));
                throw new InvalidOperationException(
                    $"Cannot delete View permission. Other permissions still exist: {otherPermissionNames}. " +
                    "Please delete these permissions first, then delete View.");
            }
        }

        _db.RolePagePermissionMappings.Remove(mapping);
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeleteRolePagePermissionMappingHandler] Saved {savedCount} entities for RolePagePermissionMapping ID: {request.Id}");

        return true;
    }
}
