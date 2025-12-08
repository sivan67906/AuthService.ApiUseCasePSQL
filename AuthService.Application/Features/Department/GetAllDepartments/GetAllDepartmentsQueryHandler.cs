using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Department.CreateDepartment;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AuthService.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Department.GetAllDepartments;
public sealed class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IAppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllDepartmentsQueryHandler(
        IAppDbContext db,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<List<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Departments.AsNoTracking()
            .Where(x => !x.IsDeleted);

        // Get current user's roles and department to apply filtering
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
                    // FIX: Get roles from UserRoleMappings instead of ASP.NET Identity UserRoles
                    // UserRoleMappings is the source of truth for our RBAC system
                    var userRoles = await _db.UserRoleMappings.AsNoTracking()
                        .Where(urm => urm.UserId == user.Id && urm.IsActive)
                        .Include(urm => urm.Role)
                        .Select(urm => urm.Role.Name)
                        .ToListAsync(cancellationToken);
                    
                    // SuperAdmin can see all departments
                    if (!userRoles.Contains(SystemRoles.SuperAdmin))
                    {
                        // Get user's department from UserRoleMappings
                        var userDepartmentId = await _db.UserRoleMappings.AsNoTracking()
                            .Where(urm => urm.UserId == user.Id && urm.IsActive)
                            .Select(urm => urm.DepartmentId)
                            .FirstOrDefaultAsync(cancellationToken);

                        // Non-SuperAdmin users can only see their own department
                        if (userDepartmentId.HasValue)
                        {
                            query = query.Where(x => x.Id == userDepartmentId.Value);
                        }
                        else
                        {
                            // If user has no department, return empty list
                            return new List<DepartmentDto>();
                        }
                    }
                }
            }
        }

        var entities = await query
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Adapt<List<DepartmentDto>>();
    }
}
