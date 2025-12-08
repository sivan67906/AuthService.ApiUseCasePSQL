using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;
public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDeletable
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    // PhoneNumber is inherited from IdentityUser<Guid>
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Indicates whether authenticator app 2FA is enabled (vs email-based 2FA)
    /// </summary>
    public bool AuthenticatorEnabled { get; set; }
    /// Secret key for TOTP authenticator app (encrypted/encoded)
    public string? AuthenticatorSecretKey { get; set; }
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();
    public ICollection<UserRoleMapping> UserRoleMappings { get; init; } = [];
}
