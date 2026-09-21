using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Domain.Repositories;
using App.Contracts.Requests.Users;
using App.Application.UseCases.Users.ChangePassword;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController(IUserRepository userRepository, ISender sender) : BaseController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct = default)
    {
        var user = await userRepository.GetByUuidAsync(UserUuid, ct);
        if (user is null)
        {
            return NotFound();
        }

        var details = await userRepository.GetDetailsByUserIdAsync(user.Id, ct);

        return Ok(new
        {
            uuid = user.Guid,
            email = user.Email,
            firstName = details?.FirstName,
            lastName = details?.LastName,
            dateOfBirth = details?.DateOfBirth,
            phoneNumber = details?.PhoneNumber
        });
    }

    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new ChangePasswordCommand(UserUuid, request.CurrentPassword, request.NewPassword),
            ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}