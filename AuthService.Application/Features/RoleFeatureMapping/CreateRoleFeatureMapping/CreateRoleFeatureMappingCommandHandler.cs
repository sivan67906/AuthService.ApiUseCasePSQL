using AuthService.Application.Common.Interfaces;
using AuthService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.RoleFeatureMapping.CreateRoleFeatureMapping;
public sealed class CreateRoleFeatureMappingCommandHandler : IRequestHandler<CreateRoleFeatureMappingCommand, RoleFeatureMappingDto>
{
    private readonly IAppDbContext _db;
    public CreateRoleFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<RoleFeatureMappingDto> Handle(CreateRoleFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        // Check if mapping already exists - including soft-deleted
        var existing = await _db.RoleFeatureMappings
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && 
                          x.FeatureId == request.FeatureId && 
                          x.DepartmentId == request.DepartmentId, cancellationToken);
                          
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                throw new InvalidOperationException("This role-feature mapping already exists in deactivated mode. Please restore the deactivated mapping instead of creating a new one.");
            }
            else
            {
                throw new InvalidOperationException("This role-feature mapping already exists");
            }
        }
        
        var entity = new Domain.Entities.RoleFeatureMapping
        {
            RoleId = request.RoleId,
            FeatureId = request.FeatureId,
            DepartmentId = request.DepartmentId,
            IsActive = request.IsActive
        };
        _db.RoleFeatureMappings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<RoleFeatureMappingDto>();
    }
}
