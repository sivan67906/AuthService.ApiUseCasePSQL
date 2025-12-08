using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetAllUsers;
public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppDbContext _db;
    public GetAllUsersQueryHandler(
        UserManager<ApplicationUser> userManager,
        IAppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }
    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            // Get departments from user role mappings
            var departments = await _db.UserRoleMappings
                .AsNoTracking()
                .Where(urm => urm.UserId == user.Id && urm.IsActive)
                .Include(urm => urm.Department)
                .Select(urm => urm.Department != null ? urm.Department.Name : null)
                .Where(d => d != null)
                .Distinct()
                .ToListAsync(cancellationToken);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsActive = user.IsActive,
                //CreatedAt = user.CreatedAt,
                //UpdatedAt = user.UpdatedAt,
                Roles = roles.ToList(),
                Departments = departments!
            });
        }
        return userDtos;
}
}
