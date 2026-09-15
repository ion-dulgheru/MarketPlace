namespace App.Contracts.Responses.Adverts;

public record AdvertPhotoResponse(
    string PhotoUrl,
    string FileName,
    string ContentType,
    bool IsPrimary
);
