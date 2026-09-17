using App.Domain.Shared;

namespace App.Domain.Errors;

public static class ContactRequestErrors
{
    public static readonly Error MessageRequired =
        Error.Validation("ContactRequest.MessageRequired", "Message is required.");

    public static readonly Error MessageTooLong =
        Error.Validation("ContactRequest.MessageTooLong", "Message must not exceed 1000 characters.");
}