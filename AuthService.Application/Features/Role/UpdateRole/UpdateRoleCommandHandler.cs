using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Role.CreateRole;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Role.UpdateRole;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;

    public UpdateRoleCommandHandler(RoleManager<ApplicationRole> roleManager, IAppDbContext db)
    {
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null || role.IsDeleted)
        {
            throw new InvalidOperationException($"Role with ID '{request.RoleId}' not found");
        }

        // Check if any changes were made
        bool hasChanges = false;
        if (role.Name != request.Name || role.Description != request.Description || role.DepartmentId != request.DepartmentId)
        {
            hasChanges = true;
        }
        
        if (!hasChanges)
        {
            throw new InvalidOperationException("No changes detected. Please modify the data before updating.");
        }

        // Check for duplicate name (case-insensitive) excluding current role and soft-deleted records
        var duplicateExists = await _db.ApplicationRoles
            .Where(r => !r.IsDeleted && r.Id != request.RoleId)
            .AnyAsync(r => r.Name!.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (duplicateExists)
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
        }

        // Check if department exists if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _db.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && !d.IsDeleted, cancellationToken);

            if (!departmentExists)
            {
                throw new InvalidOperationException($"Department with ID '{request.DepartmentId}' not found");
            }
        }

        role.Name = request.Name;
        role.NormalizedName = request.Name.ToUpperInvariant();
        role.Description = request.Description;
        role.DepartmentId = request.DepartmentId;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update role: {errors}");
        }

        return new RoleDto(
            role.Id,
            role.Name!,
            role.Description,
            role.DepartmentId,
            null
        );
    }
}

