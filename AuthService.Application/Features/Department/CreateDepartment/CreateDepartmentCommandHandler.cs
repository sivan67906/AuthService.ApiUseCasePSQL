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
        // Check for duplicate code (case-insensitive) - including soft-deleted records
        var existingByCode = await _db.Departments
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);
            
        if (existingByCode != null)
        {
            if (existingByCode.IsDeleted)
            {
                throw new InvalidOperationException($"A department with code '{request.Code}' already exists in deactivated mode. Please use a different code.");
            }
            else
            {
                throw new InvalidOperationException($"Department with code '{request.Code}' already exists");
            }
        }
        
        var entity = new Domain.Entities.Department
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description
        };
        _db.Departments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<DepartmentDto>();
}
}
