using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using App.Contracts.Requests.Users;
using App.Application.UseCases.Users.Register;
using App.Application.UseCases.Users.SignIn;
using App.Application.UseCases.Users.RefreshToken;
using App.Application.UseCases.Users.RequestPasswordReset;
using App.Application.UseCases.Users.ResetPassword;
using App.Application.UseCases.Users.VerifyEmail;
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
      [HttpPost("login")]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request,
    CancellationToken ct = default)
{
    var result = await sender.Send(new SignInCommand(request.Email, request.Password), ct);

    return result.IsFailure
        ? HandleFailure(result)
        : Ok(result.Value);
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
   
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

    // ---- Refresh-token cookie (SR-07). Not wired yet: login/refresh/logout start using these
    // ---- when the refresh token moves out of the response body (DAS plan, section 9.2).

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