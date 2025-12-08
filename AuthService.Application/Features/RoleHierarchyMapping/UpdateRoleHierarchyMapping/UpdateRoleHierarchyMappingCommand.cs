namespace AuthService.Application.Features.RoleHierarchyMapping.UpdateRoleHierarchyMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record UpdateRoleHierarchyMappingCommand : IRequest<RoleHierarchyMappingDto>
{
    public Guid Id { get; init; }
    public Guid ParentRoleId { get; init; }
    public Guid ChildRoleId { get; init; }
    public int Level { get; init; }
}

public sealed class UpdateRoleHierarchyMappingCommandHandler : IRequestHandler<UpdateRoleHierarchyMappingCommand, RoleHierarchyMappingDto>
{
    private readonly IAppDbContext _db;
    public UpdateRoleHierarchyMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<RoleHierarchyMappingDto> Handle(UpdateRoleHierarchyMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.RoleHierarchies
            .Include(rh => rh.ParentRole)
                .ThenInclude(r => r.Department)
            .Include(rh => rh.ChildRole)
            .FirstOrDefaultAsync(rh => rh.Id == request.Id, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException("Role hierarchy mapping not found");
        }
        // Validate that roles exist
        var parentRole = await _db.Roles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == request.ParentRoleId, cancellationToken);
        
        if (parentRole == null)
        {
            throw new InvalidOperationException("Parent role not found");
        }
        var childRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.Id == request.ChildRoleId, cancellationToken);
        if (childRole == null)
        {
            throw new InvalidOperationException("Child role not found");
        }

        // IMPORTANT: Validate that both roles belong to the same department (or parent has no department for system roles)
        if (parentRole.DepartmentId.HasValue && childRole.DepartmentId.HasValue && 
            parentRole.DepartmentId != childRole.DepartmentId)
        {
            throw new InvalidOperationException("Parent and child roles must belong to the same department");
        }

        // Determine the department for the hierarchy
        var departmentId = parentRole.DepartmentId ?? childRole.DepartmentId;

        if (!departmentId.HasValue)
        {
            throw new InvalidOperationException("At least one role must belong to a department. Role hierarchies cannot be created between two system roles.");
        }

        // Prevent circular hierarchy
        if (request.ParentRoleId == request.ChildRoleId)
        {
            throw new InvalidOperationException("A role cannot be its own parent");
        }
        // Check for duplicate mapping (excluding current)
        var duplicateMapping = await _db.RoleHierarchies
            .FirstOrDefaultAsync(rh => rh.Id != request.Id && 
                                      rh.ParentRoleId == request.ParentRoleId && 
                                      rh.ChildRoleId == request.ChildRoleId, cancellationToken);
        if (duplicateMapping != null)
        {
            throw new InvalidOperationException("Role hierarchy mapping already exists for these roles");
        }
        entity.ParentRoleId = request.ParentRoleId;
        entity.ChildRoleId = request.ChildRoleId;
        entity.DepartmentId = departmentId.Value;  // ← CRITICAL FIX: Update the DepartmentId
        entity.Level = request.Level;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.RoleHierarchies.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new RoleHierarchyMappingDto
        {
            Id = entity.Id,
            ParentRoleId = entity.ParentRoleId,
            ParentRoleName = parentRole.Name ?? string.Empty,
            ParentDepartmentId = parentRole.DepartmentId,
            ParentDepartmentName = parentRole.Department?.Name,
            ChildRoleId = entity.ChildRoleId,
            ChildRoleName = childRole.Name ?? string.Empty,
            ChildDepartmentId = childRole.DepartmentId,
            ChildDepartmentName = childRole.Department?.Name,
            Level = entity.Level,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
