using App.Domain.Common;
using App.Domain.ValueObjects;

namespace App.Domain.Entities;

public class Advert : PublicEntity, ISoftDeletable
{
    private Advert() { }

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public decimal SurfaceArea { get; private set; }
    public int Rooms { get; private set; }
    public int Floor { get; private set; }
    public AdvertStatus Status { get; private set; }
    public AdvertType Type { get; private set; }
    public Currency Currency { get; private set; }
    public BuildingType BuildingType { get; private set; }
    public int Levels { get; private set; }
    public int? ApartmentFloor { get; private set; }
    public string? ApartmentNumber { get; private set; }
    public string? ApartmentBlock { get; private set; }
    public decimal? GardenSquareMeters { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public Guid UserUuid { get; private set; }
    public Address Address { get; private set; } = null!;

    private readonly List<AdvertPhoto> _photos = [];
    public IReadOnlyCollection<AdvertPhoto> Photos => _photos.AsReadOnly();

    /// <inheritdoc cref="ISoftDeletable.DeletedAt"/>
    public DateTime? DeletedAt { get; private set; }

    public static Advert Create(
        Guid userUuid,
        string title,
        string description,
        decimal price,
        decimal surfaceArea,
        int rooms,
        int floor,
        AdvertType type,
        Address address,
        DateTime expiresAt,
        BuildingType buildingType = BuildingType.Apartment,
        int? levels = null,
        int? apartmentFloor = null,
        string? apartmentNumber = null,
        string? apartmentBlock = null,
        decimal? gardenSquareMeters = null,
        Currency currency = Currency.Mdl)
    {
        var computedLevels = levels.HasValue && levels.Value > 0 ? levels.Value : (floor > 0 ? floor : 1);
        var computedFloor = apartmentFloor ?? floor;

        return new Advert
        {
            UserUuid = userUuid,
            Title = title,
            Description = description,
            Price = price,
            SurfaceArea = surfaceArea,
            Rooms = rooms,
            Floor = computedFloor,
            Type = type,
            Currency = currency,
            BuildingType = buildingType,
            Levels = computedLevels,
            ApartmentFloor = apartmentFloor ?? (buildingType == BuildingType.Apartment ? computedFloor : null),
            ApartmentNumber = apartmentNumber,
            ApartmentBlock = apartmentBlock,
            GardenSquareMeters = gardenSquareMeters,
            ExpiresAt = expiresAt,
            Status = AdvertStatus.Active,
            Address = address,
            IsActive = true
        };
    }

    public void ChangeStatus(AdvertStatus status)
    {
        Status = status;
    }

    public void SoftDelete()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }

    public void AddPhoto(AdvertPhoto photo)
    {
        _photos.Add(photo);
    }

    public bool RemovePhoto(Guid photoGuid)
    {
        var photo = _photos.FirstOrDefault(p => p.Guid == photoGuid);
        if (photo is null)
        {
            return false;
        }

        _photos.Remove(photo);
        return true;
    }

    public void Update(
        string title,
        string description,
        decimal price,
        decimal surfaceArea,
        int rooms,
        int floor,
        Address address,
        int? levels = null,
        int? apartmentFloor = null,
        string? apartmentNumber = null,
        string? apartmentBlock = null,
        decimal? gardenSquareMeters = null
    )
    {
        Title = title;
        Description = description;
        Price = price;
        SurfaceArea = surfaceArea;
        Rooms = rooms;
        Floor = apartmentFloor ?? floor;
        Address = address;

        if (levels.HasValue && levels.Value > 0)
        {
            Levels = levels.Value;
        }

        if (apartmentFloor.HasValue)
        {
            ApartmentFloor = apartmentFloor.Value;
        }

        if (apartmentNumber is not null)
        {
            ApartmentNumber = apartmentNumber;
        }

        if (apartmentBlock is not null)
        {
            ApartmentBlock = apartmentBlock;
        }

        if (gardenSquareMeters.HasValue)
        {
            GardenSquareMeters = gardenSquareMeters.Value;
        }
    }
}