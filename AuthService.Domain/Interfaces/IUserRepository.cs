using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;
public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(Guid id);
}
