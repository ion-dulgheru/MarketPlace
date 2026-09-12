using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using App.Contracts.Responses;
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

    protected static ErrorDetails ToProblem(Error error) => new(error.Code, error.Message);

    protected IActionResult HandleFailure(Result result) =>
        result.Error.Type switch
        {
            ErrorType.Validation => BadRequest(ToProblem(result.Error)),
            ErrorType.NotFound => NotFound(ToProblem(result.Error)),
            ErrorType.Conflict => Conflict(ToProblem(result.Error)),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, ToProblem(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ToProblem(result.Error))
        };
}