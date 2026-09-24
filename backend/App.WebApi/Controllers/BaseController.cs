using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using App.Contracts.Responses;
using App.Domain.Shared;
using App.Domain.Enums;

namespace App.WebApi.Controllers;

public class BaseController : Controller
{
    protected Guid UserUuid => UserId ?? throw new UnauthorizedAccessException();

    protected Guid? UserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    protected static ErrorDetails ToProblem(Error error) => new(error.Code, error.Message);
    protected bool IsAdmin =>
        User.IsInRole(nameof(UserRole.Admin)) ||
        User.HasClaim(ClaimTypes.Role, nameof(UserRole.Admin)) ||
        User.HasClaim("role", nameof(UserRole.Admin)) ||
        User.Claims.Any(c => (c.Type == "role" || c.Type == ClaimTypes.Role || c.Type.EndsWith("/role")) &&
                             string.Equals(c.Value, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase)); 

    protected IActionResult HandleFailure(Result result) =>
    result.Error.Type switch
    {
        ErrorType.Validation => BadRequest(ToProblem(result.Error)),
        ErrorType.NotFound => NotFound(ToProblem(result.Error)),
        ErrorType.Conflict => Conflict(ToProblem(result.Error)),
        ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ToProblem(result.Error)),
        ErrorType.Unauthorized => Unauthorized(ToProblem(result.Error)),
        _ => StatusCode(StatusCodes.Status500InternalServerError, ToProblem(result.Error))
    };
}