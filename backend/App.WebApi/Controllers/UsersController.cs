using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Domain.Repositories;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController(IUserRepository userRepository) : BaseController
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
}