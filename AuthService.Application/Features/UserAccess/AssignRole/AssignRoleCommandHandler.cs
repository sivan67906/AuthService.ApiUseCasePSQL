using System.Security.Claims;
using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.UserAccess.AssignRole;
public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AssignRoleCommandHandler(
        UserManager<ApplicationUser> userManager, 
        RoleManager<ApplicationRole> roleManager,
        IAppDbContext db,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<bool> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.EmailId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with email '{request.EmailId}' not found");
        }
        // Verify role exists
        var role = await _roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{request.RoleName}' does not exist");
        }
        // Verify department exists
        var department = await _db.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);
        if (department == null)
        {
            throw new InvalidOperationException($"Department with ID '{request.DepartmentId}' not found");
        }
        // Verify role belongs to this department (or is a global role like SuperAdmin)
        if (role.DepartmentId.HasValue && role.DepartmentId != request.DepartmentId)
        {
            throw new InvalidOperationException($"Role '{request.RoleName}' does not belong to the selected department");
        }
        // Remove existing roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        // Add new role
        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        // Get the current user's email for tracking
        var currentUserEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "system@authservice.com";
        // Create UserRoleMapping record
        var userRoleMapping = new Domain.Entities.UserRoleMapping
        {
            UserId = user.Id,
            RoleId = role.Id,
            DepartmentId = request.DepartmentId,
            AssignedByEmail = currentUserEmail,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _db.UserRoleMappings.AddAsync(userRoleMapping, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
