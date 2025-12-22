using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.CreatePage;
public sealed class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, PageDto>
{
    private readonly IAppDbContext _db;
    public CreatePageCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PageDto> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate code (case-insensitive) - including soft-deleted records
        var existingByCode = await _db.Pages
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);
            
        if (existingByCode != null)
        {
            if (existingByCode.IsDeleted)
            {
                throw new InvalidOperationException($"A page with code '{request.Code}' already exists in deactivated mode. Please use a different code.");
            }
            else
            {
                throw new InvalidOperationException($"Page with code '{request.Code}' already exists");
            }
        }
        
        var entity = new Domain.Entities.Page
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Url = request.Url,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            MenuContext = request.MenuContext,
            IsActive = request.IsActive
        };
        _db.Pages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<PageDto>();
}


}