using AuthService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.UserAccess.GetAllUsers;
public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IAppDbContext _db;
    public GetAllUsersQueryHandler(IAppDbContext db)
    {
        _db = db;
    }
    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var entities = await _db.ApplicationUsers.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Adapt<List<UserDto>>();
    }
}
