using App.Domain.Common;

namespace App.Domain.Entities;

public class Advert : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal SurfaceArea { get; private set; }
    public int Rooms { get; private set; }
    public int Floor { get; private set; }
    public AdvertStatus Status { get; private set; }
    public AdvertType Type { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public long UserId { get; private set; }

    protected Advert() { }

    public Advert(long userId, string title, string description, decimal price, decimal surfaceArea,
                int rooms, int floor, AdvertType type, DateTime expiresAt)
{
    UserId = userId;
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