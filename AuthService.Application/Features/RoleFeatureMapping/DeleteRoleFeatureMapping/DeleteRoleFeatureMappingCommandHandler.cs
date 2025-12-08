using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.DeleteRoleFeatureMapping;

public sealed class DeleteRoleFeatureMappingCommandHandler : IRequestHandler<DeleteRoleFeatureMappingCommand, bool>
{
    private readonly IAppDbContext _db;

    public DeleteRoleFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteRoleFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var mapping = await _db.RoleFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (mapping == null)
        {
            Console.WriteLine($"[DeleteRoleFeatureMappingHandler] RoleFeatureMapping not found: {request.Id}");
            throw new KeyNotFoundException($"Role feature mapping with ID {request.Id} not found");
        }

        _db.RoleFeatureMappings.Remove(mapping);
        var savedCount = await _db.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[DeleteRoleFeatureMappingHandler] Saved {savedCount} entities for RoleFeatureMapping ID: {request.Id}");

        return true;
    }
}
