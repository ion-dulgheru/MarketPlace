using App.Domain.Shared;

namespace App.Domain.Errors;

public static class AdminErrors
{
    public static readonly Error Forbidden =
        Error.Forbidden("Admin.Forbidden", "You are not authorized to perform this action.");
}
