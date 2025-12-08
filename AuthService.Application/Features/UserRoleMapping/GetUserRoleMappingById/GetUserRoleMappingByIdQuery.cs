namespace AuthService.Application.Features.UserRoleMapping.GetUserRoleMappingById;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record GetUserRoleMappingByIdQuery(Guid Id) : IRequest<UserRoleMappingDto?>;

public sealed class GetUserRoleMappingByIdQueryHandler : IRequestHandler<GetUserRoleMappingByIdQuery, UserRoleMappingDto?>
{
    private readonly IAppDbContext _db;
    public GetUserRoleMappingByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<UserRoleMappingDto?> Handle(GetUserRoleMappingByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.UserRoleMappings
            .Include(urm => urm.User)
            .Include(urm => urm.Role)
                .ThenInclude(r => r.Department)
            .Include(urm => urm.Department)
            .Where(urm => urm.Id == request.Id)
            .Select(urm => new UserRoleMappingDto
            {
                Id = urm.Id,
                UserId = urm.UserId,
                UserEmail = urm.User.Email ?? string.Empty,
                UserName = urm.User.UserName ?? string.Empty,
                RoleId = urm.RoleId,
                RoleName = urm.Role.Name ?? string.Empty,
                DepartmentId = urm.DepartmentId,
                DepartmentName = urm.Department != null ? urm.Department.Name : urm.Role.Department != null ? urm.Role.Department.Name : null,
                AssignedByEmail = urm.AssignedByEmail,
                AssignedAt = urm.AssignedAt,
                IsActive = urm.IsActive,
                CreatedAt = urm.CreatedAt,
                UpdatedAt = urm.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
}

}