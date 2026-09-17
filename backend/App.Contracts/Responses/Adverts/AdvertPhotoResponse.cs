namespace App.Contracts.Responses.Adverts;

public record AdvertPhotoResponse(
    Guid Uuid,
    string PhotoUrl,
    bool IsPrimary
);
