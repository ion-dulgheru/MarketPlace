using App.Domain.Shared;

namespace App.Domain.Errors;

public static class AdvertErrors
{
    public static readonly Error InvalidIdentifier =
        Error.Validation("Advert.InvalidIdentifier", "Advert identifier must not be empty.");

    public static readonly Error TitleRequired =
        Error.Validation("Advert.TitleRequired", "Title is required.");

    public static readonly Error TitleTooLong =
        Error.Validation("Advert.TitleTooLong", "Title must not exceed 200 characters.");

    public static readonly Error PriceMustBePositive =
        Error.Validation("Advert.PriceMustBePositive", "Price must be greater than 0.");

    public static readonly Error SurfaceAreaMustBePositive =
        Error.Validation("Advert.SurfaceAreaMustBePositive", "Surface area must be greater than 0.");

    public static readonly Error RoomsMustBePositive =
        Error.Validation("Advert.RoomsMustBePositive", "Room count must be at least 1.");

    public static readonly Error DescriptionTooLong =
        Error.Validation("Advert.DescriptionTooLong", "Description is too long.");

    public static readonly Error InvalidType =
        Error.Validation("Advert.InvalidType", "Advert type must be 'Sale' or 'Rent'.");

    public static readonly Error InvalidStatus =
        Error.Validation("Advert.InvalidStatus", "Advert status must be 'Active', 'Sold' or 'Rented'.");

    public static readonly Error NotFound =
        Error.NotFound("Advert.NotFound", "Advert not found.");
}