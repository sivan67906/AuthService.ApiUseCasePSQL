using AuthService.Application.Common.Interfaces;

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
