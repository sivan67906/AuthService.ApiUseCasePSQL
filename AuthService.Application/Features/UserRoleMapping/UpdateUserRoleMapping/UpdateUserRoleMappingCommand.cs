namespace AuthService.Application.Features.UserRoleMapping.UpdateUserRoleMapping;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record UpdateUserRoleMappingCommand : IRequest<UserRoleMappingDto>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public Guid? DepartmentId { get; init; }
    public string AssignedByEmail { get; init; } = string.Empty;
}

public sealed class UpdateUserRoleMappingCommandHandler : IRequestHandler<UpdateUserRoleMappingCommand, UserRoleMappingDto>
{
    private readonly IAppDbContext _db;
    public UpdateUserRoleMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<UserRoleMappingDto> Handle(UpdateUserRoleMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.UserRoleMappings
            .Include(urm => urm.User)
            .Include(urm => urm.Role)
                .ThenInclude(r => r.Department)
            .Include(urm => urm.Department)
            .FirstOrDefaultAsync(urm => urm.Id == request.Id, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException("User role mapping not found");
        }
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }
        var role = await _db.Roles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }
        Domain.Entities.Department? department = null;
        if (request.DepartmentId.HasValue)
        {
            department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);
            if (department == null)
            {
                throw new InvalidOperationException("Department not found");
            }
        }
        var duplicateMapping = await _db.UserRoleMappings
            .FirstOrDefaultAsync(urm => urm.Id != request.Id && 
                                       urm.UserId == request.UserId && 
                                       urm.RoleId == request.RoleId && 
                                       urm.DepartmentId == request.DepartmentId, cancellationToken);
        if (duplicateMapping != null)
        {
            throw new InvalidOperationException("User role mapping already exists for these values");
        }
        entity.UserId = request.UserId;
        entity.RoleId = request.RoleId;
        entity.DepartmentId = request.DepartmentId;
        entity.AssignedByEmail = request.AssignedByEmail;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.UserRoleMappings.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new UserRoleMappingDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            UserEmail = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            RoleId = entity.RoleId,
            RoleName = role.Name ?? string.Empty,
            DepartmentId = entity.DepartmentId,
            DepartmentName = department?.Name ?? role.Department?.Name,
            AssignedByEmail = entity.AssignedByEmail,
            AssignedAt = entity.AssignedAt,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
