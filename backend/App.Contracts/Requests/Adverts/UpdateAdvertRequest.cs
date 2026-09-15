namespace App.Contracts.Requests.Adverts;

public record UpdateAdvertRequest(
    string? Title,
    string? Description,
    decimal? Price,
    decimal? SurfaceArea,
    int? Rooms,
    int? Floor,
    AddressRequest? Address
);