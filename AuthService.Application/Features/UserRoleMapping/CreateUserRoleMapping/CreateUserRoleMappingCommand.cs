namespace AuthService.Application.Features.UserRoleMapping.CreateUserRoleMapping;

using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public sealed record CreateUserRoleMappingCommand : IRequest<UserRoleMappingDto>
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public Guid? DepartmentId { get; init; }
    public string AssignedByEmail { get; init; } = string.Empty;
}

public sealed class CreateUserRoleMappingCommandHandler : IRequestHandler<CreateUserRoleMappingCommand, UserRoleMappingDto>
{
    private readonly IAppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public CreateUserRoleMappingCommandHandler(
        IAppDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserRoleMappingDto> Handle(CreateUserRoleMappingCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Validate role exists
        var role = await _db.Roles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        // Validate department if provided
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

        // Check if mapping already exists
        var existingMapping = await _db.UserRoleMappings
            .FirstOrDefaultAsync(urm => urm.UserId == request.UserId &&
                                       urm.RoleId == request.RoleId &&
                                       urm.DepartmentId == request.DepartmentId, cancellationToken);
        if (existingMapping != null)
        {
            throw new InvalidOperationException("User role mapping already exists");
        }

        // Create the entity
        var entity = new Domain.Entities.UserRoleMapping
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,
            AssignedByEmail = request.AssignedByEmail,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.UserRoleMappings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // Add the role to ASP.NET Identity UserRoles table
        var identityResult = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!identityResult.Succeeded)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(role.Name!))
            {
                throw new InvalidOperationException($"Failed to assign role to user: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
            }
        }

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
