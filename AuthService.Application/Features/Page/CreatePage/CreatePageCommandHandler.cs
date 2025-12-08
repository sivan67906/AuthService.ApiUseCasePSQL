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
        // Check for duplicate name
        var exists = await _db.Pages
            .AnyAsync(x => x.Name == request.Name && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Page with name '{request.Name}' already exists");
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