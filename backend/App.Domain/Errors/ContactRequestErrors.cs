using App.Domain.Shared;

namespace App.Domain.Errors;

public static class ContactRequestErrors
{
    public static readonly Error MessageRequired =
        Error.Validation("ContactRequest.MessageRequired", "Message is required.");

    public static readonly Error MessageTooLong =
        Error.Validation("ContactRequest.MessageTooLong", "Message must not exceed 1000 characters.");
    
    public static readonly Error InvalidIdentifier =
        Error.Validation("ContactRequest.InvalidIdentifier", "Contact request identifier must not be empty.");

    public static readonly Error NotFound =
        Error.NotFound("ContactRequest.NotFound", "Contact request not found.");
}