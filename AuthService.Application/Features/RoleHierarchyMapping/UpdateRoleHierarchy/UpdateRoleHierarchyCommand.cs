using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchy;
public record UpdateRoleHierarchyCommand : IRequest<RoleHierarchyDto>
{
    public Guid Id { get; set; }
    public Guid ParentRoleId { get; set; }
    public Guid ChildRoleId { get; set; }
    public int Level { get; set; }
}
public class UpdateRoleHierarchyCommandHandler : IRequestHandler<UpdateRoleHierarchyCommand, RoleHierarchyDto>
{
    private readonly IAppDbContext _db;
    public UpdateRoleHierarchyCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<RoleHierarchyDto> Handle(UpdateRoleHierarchyCommand request, CancellationToken cancellationToken)
    {
        var roleHierarchy = await _db.RoleHierarchies
            .Include(rh => rh.ParentRole)
            .Include(rh => rh.ChildRole)
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);
        if (roleHierarchy == null)
        {
            throw new Exception($"Role hierarchy with ID {request.Id} not found");
        }
        if (request.ParentRoleId == request.ChildRoleId)
        {
            throw new Exception("Parent role and child role cannot be the same");
        }
        // Validate that both roles exist
        var parentRole = await _db.Roles.FindAsync(new object[] { request.ParentRoleId }, cancellationToken);
        var childRole = await _db.Roles.FindAsync(new object[] { request.ChildRoleId }, cancellationToken);
        if (parentRole == null)
        {
            throw new Exception($"Parent role with ID {request.ParentRoleId} not found");
        }
        if (childRole == null)
        {
            throw new Exception($"Child role with ID {request.ChildRoleId} not found");
        }
        // Check for duplicate mapping (excluding current record)
        var duplicate = await _db.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id != request.Id && 
                                      rh.ParentRoleId == request.ParentRoleId && 
                                      rh.ChildRoleId == request.ChildRoleId, cancellationToken);
        if (duplicate != null)
        {
            throw new Exception("This role hierarchy mapping already exists");
        }
        roleHierarchy.ParentRoleId = request.ParentRoleId;
        roleHierarchy.ChildRoleId = request.ChildRoleId;
        roleHierarchy.Level = request.Level;
        roleHierarchy.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return new RoleHierarchyDto
        {
            Id = roleHierarchy.Id,
            ParentRoleId = roleHierarchy.ParentRoleId,
            ParentRoleName = parentRole.Name ?? string.Empty,
            ChildRoleId = roleHierarchy.ChildRoleId,
            ChildRoleName = childRole.Name ?? string.Empty,
            Level = roleHierarchy.Level,
            IsActive = roleHierarchy.IsActive,
            CreatedAt = roleHierarchy.CreatedAt,
            UpdatedAt = roleHierarchy.UpdatedAt
        };
    }
}
