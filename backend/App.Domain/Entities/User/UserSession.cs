using App.Domain.Common;

namespace App.Domain.Entities;

public class UserSession : BaseEntity
{
    private UserSession() { }

    public string RefreshTokenHash { get; private set; } = null!;
    public string JwtId { get; private set; } = null!;
    public DateTime RefreshTokenExpiry { get; private set; }
    public bool Redeemed { get; private set; }
    public long UserId { get; private set; }

    protected UserSession() { }

    public UserSession(long userId, string refreshTokenHash, string jwtId, DateTime refreshTokenExpiry)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        JwtId = jwtId;
        RefreshTokenExpiry = refreshTokenExpiry;
    }
}