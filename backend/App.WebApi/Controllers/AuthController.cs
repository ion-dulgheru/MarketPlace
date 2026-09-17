using MediatR;
using Microsoft.AspNetCore.Mvc;
using App.Contracts.Requests.Users;
using App.Application.UseCases.Users.Register;
using App.Application.UseCases.Users.SignIn;
using App.Application.UseCases.Users.RefreshToken;

namespace App.WebApi.Controllers;

[Route("api/auth")]
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
                request.PhoneNumber),
            ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
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
}