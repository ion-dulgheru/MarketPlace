using MediatR;
using Microsoft.AspNetCore.Mvc;
using App.Contracts.Requests.Users;
using App.Application.UseCases.Users.Register;

namespace App.WebApi.Controllers;

[Route("api/auth")]
public class AuthController(ISender sender) : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new RegisterUserCommand(request.Email, request.Password), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Created(string.Empty, result.Value);
    }
}