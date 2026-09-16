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

    public static UserSession Create(
        long userId,
        string refreshTokenHash,
        string jwtId,
        DateTime refreshTokenExpiry)
    {
        return new UserSession
        {
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            JwtId = jwtId,
            RefreshTokenExpiry = refreshTokenExpiry,
            Redeemed = false
        };
    }

    public bool IsValid(DateTime now) => !Redeemed && RefreshTokenExpiry > now;

    public void Redeem()
    {
        Redeemed = true;
    }
}