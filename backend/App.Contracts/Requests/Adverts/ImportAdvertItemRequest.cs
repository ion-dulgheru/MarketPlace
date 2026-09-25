namespace App.Contracts.Requests.Adverts;

public record ImportAdvertAddressRequest(
    string City,
    string Street,
    string HouseNumber,
    string? Sector
);

public record ImportAdvertItemRequest(
    string Title,
    string Description,
    decimal Price,
    decimal SurfaceArea,
    int Rooms,
    int Floor,
    string Type,
    ImportAdvertAddressRequest Address,
    int? Levels = null,
    int? ApartmentFloor = null,
    string? ApartmentNumber = null,
    string? ApartmentBlock = null,
    decimal? GardenSquareMeters = null
);
