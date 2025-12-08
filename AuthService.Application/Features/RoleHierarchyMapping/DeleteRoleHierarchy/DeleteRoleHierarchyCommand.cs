using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleHierarchyMapping.DeleteRoleHierarchy;
public record DeleteRoleHierarchyCommand(Guid Id) : IRequest<bool>;
public class DeleteRoleHierarchyCommandHandler : IRequestHandler<DeleteRoleHierarchyCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeleteRoleHierarchyCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeleteRoleHierarchyCommand request, CancellationToken cancellationToken)
    {
        var roleHierarchy = await _db.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);
        if (roleHierarchy == null)
        {
            throw new Exception($"Role hierarchy with ID {request.Id} not found");
        }
        _db.RoleHierarchies.Remove(roleHierarchy);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
