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

    public static readonly Error FileRequired =
        Error.Validation("Advert.FileRequired", "A valid photo file is required.");

    public static readonly Error FileTooLarge =
        Error.Validation("Advert.FileTooLarge", "Photo file size cannot exceed 5 MB.");

    public static readonly Error InvalidFileFormat =
        Error.Validation("Advert.InvalidFileFormat", "Only .jpg, .jpeg, .png, and .webp images are allowed.");

    public static readonly Error PhotoNotFound =
        Error.NotFound("Advert.PhotoNotFound", "Photo not found.");

    public static readonly Error NotActive =
        Error.NotFound("Advert.NotActive", "Listing is not active.");
}