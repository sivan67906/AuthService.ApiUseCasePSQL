namespace AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchyMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record DeleteRoleHierarchyMappingCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteRoleHierarchyMappingCommandHandler : IRequestHandler<DeleteRoleHierarchyMappingCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeleteRoleHierarchyMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeleteRoleHierarchyMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException("Role hierarchy mapping not found");
        }
        _db.RoleHierarchies.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
