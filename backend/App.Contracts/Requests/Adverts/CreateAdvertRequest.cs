namespace App.Contracts.Requests.Adverts;

public record CreateAdvertRequest(
    string Title,
    string Description,
    decimal Price,
    decimal SurfaceArea,
    int Rooms,
    int Floor,
    string Type 
);