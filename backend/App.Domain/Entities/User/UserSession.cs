using App.Domain.Common;

namespace App.Domain.Entities;

public class UserSession : BaseEntity
{

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

    public static UserSession Create(long userId, string refreshTokenHash, string jwtId, DateTime refreshTokenExpiry)
    {
        return new UserSession(userId, refreshTokenHash, jwtId, refreshTokenExpiry);
    }

    public bool IsValid(DateTime now)
    {
        return !Redeemed && RefreshTokenExpiry > now;
    }

    public void Redeem()
    {
        Redeemed = true;
    }
}