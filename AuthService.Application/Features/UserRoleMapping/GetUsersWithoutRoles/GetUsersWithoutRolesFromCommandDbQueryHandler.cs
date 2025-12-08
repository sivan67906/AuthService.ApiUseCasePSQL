using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserRoleMapping.GetUsersWithoutRoles;

public sealed class GetUsersWithoutRolesFromCommandDbQueryHandler
    : IRequestHandler<GetUsersWithoutRolesFromCommandDbQuery, List<UserWithoutRoleDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppDbContext _db;

    public GetUsersWithoutRolesFromCommandDbQueryHandler(
        UserManager<ApplicationUser> userManager,
        IAppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<List<UserWithoutRoleDto>> Handle(
        GetUsersWithoutRolesFromCommandDbQuery request,
        CancellationToken cancellationToken)
    {
        // Get all user IDs that have role mappings from Command DB for immediate consistency
        var userIdsWithRoles = await _db.UserRoleMappings
            .AsNoTracking()
            .Where(urm => urm.IsActive)
            .Select(urm => urm.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get active users without role mappings
        var usersWithoutRoles = await _userManager.Users
            .Where(u => u.IsActive && !userIdsWithRoles.Contains(u.Id))
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return usersWithoutRoles.Select(u => new UserWithoutRoleDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive
        }).ToList();
    }
}