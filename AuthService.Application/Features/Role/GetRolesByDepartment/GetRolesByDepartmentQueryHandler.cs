using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Role.CreateRole;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Role.GetRolesByDepartment;

public sealed class GetRolesByDepartmentQueryHandler : IRequestHandler<GetRolesByDepartmentQuery, List<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;
    
    public GetRolesByDepartmentQueryHandler(RoleManager<ApplicationRole> roleManager, IAppDbContext db)
    {
        _roleManager = roleManager;
        _db = db;
    }
    
    public async Task<List<RoleDto>> Handle(GetRolesByDepartmentQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ApplicationRole> query = _roleManager.Roles.AsNoTracking()
            .Include(r => r.Department);

        // If DepartmentId is null, return ALL roles (System wide - All Departments)
        // If DepartmentId is provided, filter by that specific department
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(r => r.DepartmentId == request.DepartmentId.Value);
        }

        var roles = await query
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto(
            r.Id,
            r.Name!,
            r.Description,
            r.DepartmentId,
            r.Department?.Name
        )).ToList();
    }
}
