using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Auth.RevokeToken;
public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly IAppDbContext _db;
    public RevokeTokenCommandHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _db.Set<UserRefreshToken>()
            .Where(x => x.Token == request.RefreshToken && !x.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);
        if (token is null)
        {
            return false;
        }
        token.IsRevoked = true;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
}
}
