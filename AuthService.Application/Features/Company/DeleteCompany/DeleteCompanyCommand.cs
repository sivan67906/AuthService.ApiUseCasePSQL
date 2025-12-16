using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Company.DeleteCompany;

public sealed record DeleteCompanyCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, bool>
{
    private readonly IAppDbContext _db;

    public DeleteCompanyCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.Companies
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException("Company not found.");
        }

        // Check if company has child companies
        var hasChildren = await _db.Companies
            .AnyAsync(x => x.ParentCompanyId == request.Id, cancellationToken);

        if (hasChildren)
        {
            throw new InvalidOperationException("Cannot delete company with child companies. Please reassign or delete child companies first.");
        }

        // Soft delete - handled by DbContext SaveChangesAsync
        _db.Companies.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
