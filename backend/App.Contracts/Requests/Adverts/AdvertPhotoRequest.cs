namespace App.Contracts.Requests.Adverts;

public record AdvertPhotoRequest(
    string PhotoUrl,
    bool IsPrimary
);
