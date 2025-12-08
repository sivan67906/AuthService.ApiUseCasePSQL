using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.PageFeatureMapping.DeletePageFeatureMapping;
public sealed class DeletePageFeatureMappingCommandHandler : IRequestHandler<DeletePageFeatureMappingCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeletePageFeatureMappingCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(DeletePageFeatureMappingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.PageFeatureMappings
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity == null)
        {
            Console.WriteLine($"[DeletePageFeatureMappingHandler] PageFeatureMapping not found: {request.Id}");
            return false;
        }
        _db.PageFeatureMappings.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
}
}
