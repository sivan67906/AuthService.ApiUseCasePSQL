namespace AuthService.Application.Features.RoleHierarchyMapping.CreateRoleHierarchyMapping;

using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record CreateRoleHierarchyMappingCommand : IRequest<RoleHierarchyMappingDto>
{
    public Guid ParentRoleId { get; init; }
    public Guid ChildRoleId { get; init; }
    public int Level { get; init; }
}

public sealed class CreateRoleHierarchyMappingCommandHandler
    : IRequestHandler<CreateRoleHierarchyMappingCommand, RoleHierarchyMappingDto>
{
    private readonly IAppDbContext _db;

    public CreateRoleHierarchyMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleHierarchyMappingDto> Handle(CreateRoleHierarchyMappingCommand request, CancellationToken cancellationToken)
    {
        // Validate that parent and child roles exist
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
        // Use the department from whichever role has it (prioritize parent role)
        var departmentId = parentRole.DepartmentId ?? childRole.DepartmentId;

        if (!departmentId.HasValue)
        {
            throw new InvalidOperationException("At least one role must belong to a department. Role hierarchies cannot be created between two system roles.");
        }

        // Check if mapping already exists (excluding soft-deleted)
        var existingMapping = await _db.RoleHierarchies
            .Where(rh => !rh.IsDeleted)
            .FirstOrDefaultAsync(rh => rh.ParentRoleId == request.ParentRoleId &&
                                       rh.ChildRoleId == request.ChildRoleId, cancellationToken);

        if (existingMapping != null)
        {
            throw new InvalidOperationException("Role hierarchy mapping already exists");
        }

        // Prevent circular hierarchy
        if (request.ParentRoleId == request.ChildRoleId)
        {
            throw new InvalidOperationException("A role cannot be its own parent");
        }

        var entity = new Domain.Entities.RoleHierarchy
        {
            Id = Guid.NewGuid(),
            ParentRoleId = request.ParentRoleId,
            ChildRoleId = request.ChildRoleId,
            DepartmentId = departmentId.Value,  // ← CRITICAL FIX: Set the DepartmentId
            Level = request.Level,
            CreatedAt = DateTime.UtcNow
        };

        _db.RoleHierarchies.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new RoleHierarchyMappingDto
        {
            Id = entity.Id,
            DepartmentId = entity.DepartmentId,
            DepartmentName = parentRole.Department?.Name ?? string.Empty,
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