using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Domain.Repositories;
using App.Contracts.Requests.Users;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController(IUserRepository userRepository, IUnitOfWork unitOfWork) : BaseController
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
        var user = await userRepository.GetByUuidAsync(UserUuid, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Current password is incorrect.");
        }

        user.ResetPassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}