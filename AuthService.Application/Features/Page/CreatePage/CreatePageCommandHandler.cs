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
        // Check for duplicate name (case-insensitive) - including soft-deleted records
        var existing = await _db.Pages
            .IgnoreQueryFilters() // Include deleted records
            .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                throw new InvalidOperationException($"A page with name '{request.Name}' already exists in deactivated mode. Please use a different name.");
            }
            else
            {
                throw new InvalidOperationException($"Page with name '{request.Name}' already exists");
            }
        }
        
        var entity = new Domain.Entities.Page
        {
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