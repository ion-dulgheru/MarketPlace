namespace App.Domain.Entites;
using App.Domain.Common;
public class UserSession : BaseEntity
{

    public string RefreshTokenHash {get; private set; }= null!;
    public string JwtId { get; private set; }= null!;
    public DateTime RefreshTokenExpiry {get; private set; }
    public bool Reedemed {get; private set; } = false;
    public long UserId {get; private set; }
}