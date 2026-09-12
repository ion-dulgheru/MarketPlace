using App.Domain.Shared;

namespace App.Domain.Entities;

public static class AdvertErrors
{
    public static readonly Error InvalidType =
        Error.Validation("Advert.InvalidType", "Advert type must be 'Sale' or 'Rent'.");

    public static readonly Error NotFound =
        Error.NotFound("Advert.NotFound", "Advert not found.");
}