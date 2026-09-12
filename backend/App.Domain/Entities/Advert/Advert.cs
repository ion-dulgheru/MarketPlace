using App.Domain.Common;

namespace App.Domain.Entities;

public class Advert : BaseEntity
{
    public string Title { get; private set; }= null!;
    public string Description { get; private set; }= null!;
    public decimal Price { get; private set; }
    public decimal SurfaceArea { get; private set; }
    public int Rooms { get; private set; }
    public int Floor { get; private set; }
    public AdvertStatus Status { get; private set; }
    public AdvertType Type { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public Guid UserUuid { get; private set; }   // was long UserId

    protected Advert() { }

    public Advert(Guid userUuid, string title, string description, decimal price, decimal surfaceArea,
                  int rooms, int floor, AdvertType type, DateTime expiresAt)
    {
        UserUuid = userUuid;
        Title = title;
        Description = description;
        Price = price;
        SurfaceArea = surfaceArea;
        Rooms = rooms;
        Floor = floor;
        Type = type;
        ExpiresAt = expiresAt;
        Status = AdvertStatus.Active;
    }
}