using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Feature.CreateFeature;
public sealed class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, FeatureDto>
{
    private readonly IAppDbContext _db;
    public CreateFeatureCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<FeatureDto> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        var exists = await _db.Features
            .AnyAsync(x => x.Name == request.Name && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Feature with name '{request.Name}' already exists");
        }
        var entity = new Domain.Entities.Feature
        {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            IsMainMenu = request.IsMainMenu,
            ParentFeatureId = request.ParentFeatureId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };
        _db.Features.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<FeatureDto>();
}


}