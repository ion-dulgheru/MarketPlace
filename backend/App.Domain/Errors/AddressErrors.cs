using App.Domain.Shared;

namespace App.Domain.Errors;

public static class AddressErrors
{
    public static readonly Error RequiredField =
        Error.Validation("Address.RequiredField", "All address fields are required.");
}