using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Department.CreateDepartment;
public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IAppDbContext _db;
    public CreateDepartmentCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name (case-insensitive) - including soft-deleted records
        var existing = await _db.Departments
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                throw new InvalidOperationException($"A department with name '{request.Name}' already exists in deactivated mode. Please use a different name.");
            }
            else
            {
                throw new InvalidOperationException($"Department with name '{request.Name}' already exists");
            }
        }
        
        var entity = new Domain.Entities.Department
        {
            Name = request.Name,
            Description = request.Description
        };
        _db.Departments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<DepartmentDto>();
}
}
