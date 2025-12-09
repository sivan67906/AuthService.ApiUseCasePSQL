using AuthService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Role.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAppDbContext _db;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IAppDbContext db,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _roleManager = roleManager;
        _db = db;
        _logger = logger;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role already exists (case-insensitive) - including soft-deleted records
        var existingRole = await _db.ApplicationRoles
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(r => r.Name!.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (existingRole != null)
        {
            if (existingRole.IsDeleted)
            {
                throw new InvalidOperationException($"A role with name '{request.Name}' already exists in deactivated mode. Please use a different name.");
            }
            else
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }
        }

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _db.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && !d.IsDeleted, cancellationToken);

            if (!departmentExists)
            {
                throw new InvalidOperationException($"Department with ID '{request.DepartmentId.Value}' not found");
            }
        }

        var role = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description,
            DepartmentId = request.DepartmentId,
            IsActive = true
        };

        // Create role via RoleManager (handles all database operations)
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        _logger.LogInformation("Role {RoleId} ({RoleName}) created successfully", role.Id, role.Name);

        string? departmentName = null;
        if (request.DepartmentId.HasValue)
        {
            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);

            departmentName = department?.Name;
        }

        return new RoleDto(role.Id, role.Name!, role.Description, role.DepartmentId, departmentName);
    }
}