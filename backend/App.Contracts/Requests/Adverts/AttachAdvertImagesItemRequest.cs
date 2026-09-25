namespace App.Contracts.Requests.Adverts;

public record AttachAdvertImagesItemRequest(
    string Title,
    decimal Price,
    decimal SurfaceArea,
    int Floor,
    List<string> ImagePaths
);
