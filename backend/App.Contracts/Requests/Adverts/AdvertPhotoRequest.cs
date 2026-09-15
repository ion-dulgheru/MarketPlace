namespace App.Contracts.Requests.Adverts;

public record AdvertPhotoRequest(
    string PhotoUrl,
    string FileName,
    string ContentType,
    bool IsPrimary
);
