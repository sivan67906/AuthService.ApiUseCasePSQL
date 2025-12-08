using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.CreatePageFeatureMapping;
public sealed class CreatePageFeatureMappingCommandHandler : IRequestHandler<CreatePageFeatureMappingCommand, PageFeatureMappingDto>
{
    private readonly IAppDbContext _db;
    public CreatePageFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<PageFeatureMappingDto> Handle(CreatePageFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        // Check if mapping already exists
        var exists = await _db.PageFeatureMappings
            .AnyAsync(x => x.PageId == request.PageId && x.FeatureId == request.FeatureId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("This page-feature mapping already exists");
        }
        var entity = new Domain.Entities.PageFeatureMapping
        {
            PageId = request.PageId,
            FeatureId = request.FeatureId
        };
        _db.PageFeatureMappings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Adapt<PageFeatureMappingDto>();
}


}