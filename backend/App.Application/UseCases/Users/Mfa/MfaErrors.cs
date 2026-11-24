using App.Domain.Shared;

namespace App.Application.UseCases.Users.Mfa;

public static class MfaErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("User.NotFound", "User was not found.");

    public static readonly Error AlreadyEnabled =
        Error.Conflict("Mfa.AlreadyEnabled", "Two-step verification is already enabled.");

    public static readonly Error NotEnabled =
        Error.Conflict("Mfa.NotEnabled", "Two-step verification is not enabled.");

    public static readonly Error SetupNotStarted =
        Error.Conflict("Mfa.SetupNotStarted", "Start the two-step verification setup first.");

    /// <summary>Validation (400), not Unauthorized: the client treats 401 as an expired access token.</summary>
    public static readonly Error InvalidCode = Error.Validation(
        "Mfa.InvalidCode",
        "The code is invalid or was already used. Wait for a new code and try again.");

    /// <summary>The stored secret cannot be decrypted, so no code can be checked.</summary>
    public static readonly Error SecretUnavailable = Error.Failure(
        "Mfa.SecretUnavailable",
        "Two-step verification is temporarily unavailable. Contact support.");

    public static readonly Error InvalidLoginToken = Error.Unauthorized(
        "Auth.InvalidMfaToken",
        "Your login session has expired. Please log in again.");

    public static readonly Error AccountLocked = Error.Forbidden(
        "Auth.AccountLocked",
        "This account is temporarily locked due to too many failed login attempts. Try again later.");
}
