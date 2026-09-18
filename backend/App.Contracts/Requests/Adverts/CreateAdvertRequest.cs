namespace App.Contracts.Requests.Adverts;

public record CreateAdvertRequest(
    string Title,
    string Description,
    decimal Price,
    decimal SurfaceArea,
    int Rooms,
    int Floor,
    string Type,
    AddressRequest Address,
    List<AdvertPhotoRequest>? Photos = null,
    string BuildingType = "Apartment",
    int Levels = 1,
    int? ApartmentFloor = null,
    string? ApartmentNumber = null,
    string? ApartmentBlock = null,
    decimal? GardenSquareMeters = null
);