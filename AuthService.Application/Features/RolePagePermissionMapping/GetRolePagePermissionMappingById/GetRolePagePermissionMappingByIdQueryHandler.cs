using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RolePagePermissionMapping.GetRolePagePermissionMappingById;

public sealed class GetRolePagePermissionMappingByIdQueryHandler : IRequestHandler<GetRolePagePermissionMappingByIdQuery, RolePagePermissionMappingDto>
{
    private readonly IAppDbContext _db;

    public GetRolePagePermissionMappingByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RolePagePermissionMappingDto> Handle(GetRolePagePermissionMappingByIdQuery request, CancellationToken cancellationToken)
    {
        var mapping = await _db.RolePagePermissionMappings.AsNoTracking()
            .Include(rppm => rppm.Role)
            .Include(rppm => rppm.Page)
            .Include(rppm => rppm.Permission)
            .Include(rppm => rppm.Department)
            .Where(rppm => rppm.Id == request.Id && !rppm.IsDeleted)
            .Select(rppm => new RolePagePermissionMappingDto
            {
                Id = rppm.Id,
                RoleId = rppm.RoleId,
                RoleName = rppm.Role.Name ?? string.Empty,
                PageId = rppm.PageId,
                PageName = rppm.Page.Name,
                PermissionId = rppm.PermissionId,
                PermissionName = rppm.Permission.Name,
                DepartmentId = rppm.DepartmentId,
                DepartmentName = rppm.Department != null ? rppm.Department.Name : null,
                IsActive = rppm.IsActive,
                CreatedAt = rppm.CreatedAt,
                UpdatedAt = rppm.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mapping == null)
        {
            throw new KeyNotFoundException($"Role page permission mapping with ID {request.Id} not found");
        }

        return mapping;
    }
}
