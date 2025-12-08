namespace AuthService.Application.Features.UserRoleMapping.DeleteUserRoleMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record DeleteUserRoleMappingCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteUserRoleMappingCommandHandler : IRequestHandler<DeleteUserRoleMappingCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeleteUserRoleMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeleteUserRoleMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.UserRoleMappings
            .FirstOrDefaultAsync(urm => urm.Id == request.Id, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException("User role mapping not found");
        }
        _db.UserRoleMappings.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
