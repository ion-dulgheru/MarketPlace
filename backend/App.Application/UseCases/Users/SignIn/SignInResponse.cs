namespace App.Application.UseCases.Users.SignIn;

/// <summary>
/// Login result. Shared contract for the refresh-cookie and MFA work (DAS plan, section 3.3):
/// <list type="bullet">
/// <item>Normal login: <see cref="AccessToken"/> set, <see cref="MfaRequired"/> = false.</item>
/// <item>MFA enabled: no tokens, <see cref="MfaRequired"/> = true and <see cref="MfaToken"/> set;
/// the client then calls POST /api/auth/login/mfa.</item>
/// </list>
/// <see cref="RefreshToken"/> is still returned in the body for now; it moves to the
/// HttpOnly cookie (AuthController.SetRefreshCookie) and is then removed from this record.
/// </summary>
public record SignInResponse(
    string? AccessToken,
    string? RefreshToken,
    bool MfaRequired = false,
    string? MfaToken = null);