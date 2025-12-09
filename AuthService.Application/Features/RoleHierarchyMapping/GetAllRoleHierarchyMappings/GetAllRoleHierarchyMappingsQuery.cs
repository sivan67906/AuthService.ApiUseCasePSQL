namespace AuthService.Application.Features.RoleHierarchyMapping.GetAllRoleHierarchyMappings;
using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed record GetAllRoleHierarchyMappingsQuery : IRequest<List<RoleHierarchyMappingDto>>;

public sealed class GetAllRoleHierarchyMappingsQueryHandler : IRequestHandler<GetAllRoleHierarchyMappingsQuery, List<RoleHierarchyMappingDto>>
{
    private readonly IAppDbContext _db;
    public GetAllRoleHierarchyMappingsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<RoleHierarchyMappingDto>> Handle(GetAllRoleHierarchyMappingsQuery request, CancellationToken cancellationToken)
    {
        return await _db.RoleHierarchies
            .Include(rh => rh.Department) // Include the main Department
            .Include(rh => rh.ParentRole)
                .ThenInclude(r => r.Department)
            .Include(rh => rh.ChildRole)
            .Select(rh => new RoleHierarchyMappingDto
            {
                Id = rh.Id,
                DepartmentId = rh.DepartmentId,
                DepartmentName = rh.Department.Name,
                ParentRoleId = rh.ParentRoleId,
                ParentRoleName = rh.ParentRole.Name ?? string.Empty,
                ParentDepartmentId = rh.ParentRole.DepartmentId,
                ParentDepartmentName = rh.ParentRole.Department != null ? rh.ParentRole.Department.Name : null,
                ChildRoleId = rh.ChildRoleId,
                ChildRoleName = rh.ChildRole.Name ?? string.Empty,
                ChildDepartmentId = rh.ChildRole.DepartmentId,
                ChildDepartmentName = rh.ChildRole.Department != null ? rh.ChildRole.Department.Name : null,
                Level = rh.Level,
                IsActive = rh.IsActive,
                CreatedAt = rh.CreatedAt,
                UpdatedAt = rh.UpdatedAt
            })
            .OrderByDescending(rh => rh.UpdatedAt ?? rh.CreatedAt)
            .ToListAsync(cancellationToken);
}

}