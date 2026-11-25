using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using App.Contracts.Requests.Users;
using App.Contracts.Responses.Users;
using App.Domain.Shared;
using App.Application.UseCases.Users.Logout;
using App.Application.UseCases.Users.Register;
using App.Application.UseCases.Users.SignIn;
using App.Application.UseCases.Users.RefreshToken;
using App.Application.UseCases.Users.RequestPasswordReset;
using App.Application.UseCases.Users.ResendEmailVerification;
using App.Application.UseCases.Users.ResetPassword;
using App.Application.UseCases.Users.VerifyEmail;
using App.Application.UseCases.Users.Mfa.DisableMfa;
using App.Application.UseCases.Users.Mfa.EnableMfa;
using App.Application.UseCases.Users.Mfa.SetupMfa;
using App.Application.UseCases.Users.Mfa.VerifyMfaLogin;
namespace App.WebApi.Controllers;

[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(ISender sender) : BaseController
{
    [HttpPost("register")]
public async Task<IActionResult> Register(
    [FromBody] RegisterUserRequest request,
    CancellationToken ct = default)
{
    var result = await sender.Send(
        new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.CaptchaToken),
        ct);

    return result.IsFailure
        ? HandleFailure(result)
        : Ok();
}

    [HttpPost("verify-email")]
public async Task<IActionResult> VerifyEmail(
    [FromBody] VerifyEmailRequest request,
    CancellationToken ct = default)
{
    var result = await sender.Send(new VerifyEmailCommand(request.Token), ct);

    return result.IsFailure
        ? HandleFailure(result)
        : Ok();
}
    [HttpPost("verification-emails")]
    public async Task<IActionResult> ResendEmailVerification(
        [FromBody] ResendEmailVerificationRequest request,
        CancellationToken ct = default)
    {
        await sender.Send(new ResendEmailVerificationCommand(request.Email), ct);
        return Accepted();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new SignInCommand(request.Email, request.Password), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : SignedIn(result.Value);
    }

    /// <summary>Second login step for accounts with MFA: exchanges the mfaToken and a TOTP code for tokens.</summary>
    [HttpPost("login/mfa")]
    public async Task<IActionResult> LoginMfa(
        [FromBody] MfaLoginRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new VerifyMfaLoginCommand(request.MfaToken, request.Code), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : SignedIn(result.Value);
    }

    [Authorize]
    [HttpPost("mfa/setup")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> SetupMfa(
        [FromBody] MfaSetupRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new SetupMfaCommand(UserUuid, request.CurrentPassword), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [Authorize]
    [HttpPost("mfa/enable")]
    public async Task<IActionResult> EnableMfa(
        [FromBody] MfaCodeRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new EnableMfaCommand(UserUuid, request.Code), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [Authorize]
    [HttpPost("mfa/disable")]
    public async Task<IActionResult> DisableMfa(
        [FromBody] MfaCodeRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new DisableMfaCommand(UserUuid, request.Code), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

      [HttpPost("password-reset-requests")]
public async Task<IActionResult> RequestPasswordReset(
    [FromBody] RequestPasswordResetRequest request,
    CancellationToken ct = default)
{
    await sender.Send(new RequestPasswordResetCommand(request.Email), ct);
    return Accepted();
}

[HttpPost("password-resets")]
public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordRequest request,
    CancellationToken ct = default)
{
    var result = await sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword), ct);
    return result.IsFailure ? HandleFailure(result) : Ok();
}
   
    // No body: the refresh token comes only from the HttpOnly cookie and is rotated on every call.
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct = default)
    {
        var refreshToken = ReadRefreshCookie();
        if (refreshToken is null)
        {
            return Unauthorized(ToProblem(MissingRefreshToken));
        }

        var result = await sender.Send(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsFailure)
        {
            ClearRefreshCookie();
            return HandleFailure(result);
        }

        SetRefreshCookie(result.Value.RefreshToken);
        return Ok(new AccessTokenResponse(result.Value.AccessToken));
    }

    // Not [Authorize]: the access token may already be expired, the cookie is enough to find the session.
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        var refreshToken = ReadRefreshCookie();
        if (refreshToken is not null)
        {
            await sender.Send(new LogoutCommand(refreshToken), ct);
        }

        ClearRefreshCookie();
        return NoContent();
    }

    // Login and login/mfa: the refresh token goes only into the cookie, never into the body.
    private OkObjectResult SignedIn(SignInResponse response)
    {
        if (response.RefreshToken is not null)
        {
            SetRefreshCookie(response.RefreshToken);
        }

        return Ok(new LoginResponse(response.AccessToken, response.MfaRequired, response.MfaToken));
    }

    private static readonly Error MissingRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "Invalid or expired refresh token.");

    // ---- Refresh-token cookie (SR-07).

    internal const string RefreshCookieName = "openkey_rt";
    private const string RefreshCookiePath = "/api/auth";
    private static readonly TimeSpan RefreshCookieLifetime = TimeSpan.FromDays(30);

    // SameSite=None is required: the frontend and the API live on different azurewebsites.net
    // subdomains, which browsers treat as different sites (Public Suffix List). Strict/Lax cookies
    // would never be sent with the API fetch. CORS must therefore allow only the exact frontend origin.
    private CookieOptions RefreshCookieOptions(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = RefreshCookiePath,
        Expires = expires,
        IsEssential = true,
    };

    private void SetRefreshCookie(string refreshToken) =>
        Response.Cookies.Append(
            RefreshCookieName,
            refreshToken,
            RefreshCookieOptions(DateTimeOffset.UtcNow.Add(RefreshCookieLifetime)));

    private void ClearRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, RefreshCookieOptions(expires: null));

    private string? ReadRefreshCookie() =>
        Request.Cookies.TryGetValue(RefreshCookieName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
}