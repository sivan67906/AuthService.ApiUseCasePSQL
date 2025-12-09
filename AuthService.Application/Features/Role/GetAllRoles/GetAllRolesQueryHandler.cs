using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Role.CreateRole;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Role.GetAllRoles;
public sealed class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;
    public GetAllRolesQueryHandler(RoleManager<ApplicationRole> roleManager, IAppDbContext db)
    {
        _roleManager = roleManager;
        _db = db;
    }
    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles.AsNoTracking()
            .Include(r => r.Department)
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
