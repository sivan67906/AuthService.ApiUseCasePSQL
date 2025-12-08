using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Department.CreateDepartment;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AuthService.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Department.GetDepartment;
public sealed class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, DepartmentDto?>
{
    private readonly IAppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetDepartmentQueryHandler(
        IAppDbContext db,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.Departments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        // Apply department-based access control
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser?.Identity?.IsAuthenticated == true)
        {
            var userEmail = currentUser.FindFirst(ClaimTypes.Email)?.Value 
                ?? currentUser.FindFirst("email")?.Value;
            
            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user != null)
                {
                    // Get roles from UserRoleMappings (source of truth for RBAC)
                    var userRoles = await _db.UserRoleMappings.AsNoTracking()
                        .Where(urm => urm.UserId == user.Id && urm.IsActive)
                        .Include(urm => urm.Role)
                        .Select(urm => urm.Role.Name)
                        .ToListAsync(cancellationToken);
                    
                    // SuperAdmin can view any department
                    if (!userRoles.Contains(SystemRoles.SuperAdmin))
                    {
                        // Get user's department
                        var userDepartmentId = await _db.UserRoleMappings.AsNoTracking()
                            .Where(urm => urm.UserId == user.Id && urm.IsActive)
                            .Select(urm => urm.DepartmentId)
                            .FirstOrDefaultAsync(cancellationToken);

                        // Non-SuperAdmin users can only view their own department
                        if (!userDepartmentId.HasValue || entity.Id != userDepartmentId.Value)
                        {
                            return null; // Not authorized to view this department
                        }
                    }
                }
            }
        }

        return entity.Adapt<DepartmentDto>();
    }
}
