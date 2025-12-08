namespace AuthService.Application.Features.UserRoleMapping.GetAllUserRoleMappings;

using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
public sealed record GetAllUserRoleMappingsQuery : IRequest<List<UserRoleMappingDto>>;
public sealed class GetAllUserRoleMappingsQueryHandler : IRequestHandler<GetAllUserRoleMappingsQuery, List<UserRoleMappingDto>>
{
    private readonly IAppDbContext _db;
    public GetAllUserRoleMappingsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<UserRoleMappingDto>> Handle(GetAllUserRoleMappingsQuery request, CancellationToken cancellationToken)
    {
        var mappings = await _db.UserRoleMappings
            .AsNoTracking()
            .Include(urm => urm.User)
            .Include(urm => urm.Role)
                .ThenInclude(r => r.Department)
            .Include(urm => urm.Department)
            .ToListAsync(cancellationToken);
        return mappings.Select(urm => new UserRoleMappingDto
        {
            Id = urm.Id,
            UserId = urm.UserId,
            UserEmail = urm.User?.Email ?? string.Empty,
            UserName = urm.User?.UserName ?? string.Empty,
            RoleId = urm.RoleId,
            RoleName = urm.Role?.Name ?? string.Empty,
            DepartmentId = urm.DepartmentId,
            DepartmentName = urm.Department?.Name ?? (urm.Role?.Department?.Name ?? null),
            AssignedByEmail = urm.AssignedByEmail ?? string.Empty,
            AssignedAt = urm.AssignedAt,
            IsActive = urm.IsActive,
            CreatedAt = urm.CreatedAt,
            UpdatedAt = urm.UpdatedAt
        })
        .OrderBy(urm => urm.DepartmentName ?? string.Empty)
        .ThenBy(urm => urm.RoleName)
        .ThenBy(urm => urm.UserName)
        .ToList();
}

}