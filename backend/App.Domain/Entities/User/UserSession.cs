using App.Domain.Common;

namespace App.Domain.Entities;

/// <summary>
/// Internal-only entity. Stores JWT/refresh token data for a user login session.
/// Intentionally inherits BaseEntity (not PublicEntity) — no endpoint should ever
/// expose a session by a public identifier (Guid).
/// </summary>
public class UserSession : BaseEntity
{
    public string RefreshTokenHash { get; private set; } = null!;
    public string JwtId { get; private set; } = null!;
    public DateTime RefreshTokenExpiry { get; private set; }
    public bool Redeemed { get; private set; } = false;
    public long UserId { get; private set; }
}