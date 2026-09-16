using App.Domain.Common;

namespace App.Domain.Entities;

public class AdvertPhoto : PublicEntity
{
    private AdvertPhoto() { }

    public string PhotoUrl { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    
    public long AdvertId { get; private set; }
    public Advert Advert { get; private set; } = null!;

    public static AdvertPhoto Create(
        string photoUrl,
        bool isPrimary)
    {
        return new AdvertPhoto
        {
            PhotoUrl = photoUrl,
            IsPrimary = isPrimary
        };
    }
}