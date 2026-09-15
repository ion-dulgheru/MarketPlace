namespace App.Contracts.Responses.Adverts;

public record AdvertResponse(
    Guid Guid,
    string Title,
    string Description,
    decimal Price,
    decimal SurfaceArea,
    int Rooms,
    int Floor,
    string Status,
    string Type,
    DateTime CreatedDate
);