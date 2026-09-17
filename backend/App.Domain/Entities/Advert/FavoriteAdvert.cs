using App.Domain.Common;

namespace App.Domain.Entities;

public class FavoriteAdvert : BaseEntity
{
    private FavoriteAdvert() { }

    public Guid UserUuid { get; private set; }
    public long AdvertId { get; private set; }
    public Advert Advert { get; private set; } = null!;

    public static FavoriteAdvert Create(Guid userUuid, long advertId)
    {
        return new FavoriteAdvert
        {
            UserUuid = userUuid,
            AdvertId = advertId,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
    }
}
