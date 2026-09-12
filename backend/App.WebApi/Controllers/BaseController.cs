using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using App.Domain.Shared;

namespace App.WebApi.Controllers;

public class BaseController : Controller
{
    protected Guid UserUuid => UserId ?? throw new UnauthorizedAccessException();

    protected Guid? UserId
    {
        get
        {
            var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    protected IActionResult HandleFailure(Result result) =>
        result.Error.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error)
        };
}