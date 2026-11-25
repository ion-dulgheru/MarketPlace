using App.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace App.WebApi.Infrastructure;

/// <summary>
/// Single responsibility: answer 400 when the JSON body is missing or cannot be parsed,
/// so actions never receive a null request.
/// </summary>
public sealed class InvalidBodyFilter : IActionFilter
{
    private static readonly ErrorDetails InvalidBody =
        new("Request.InvalidBody", "The request body is missing or is not valid JSON.");

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var bodyIsMissing = context.ActionDescriptor.Parameters.Any(parameter =>
            parameter.BindingInfo?.BindingSource == BindingSource.Body
            && (!context.ActionArguments.TryGetValue(parameter.Name, out var value) || value is null));

        if (bodyIsMissing)
        {
            context.Result = new BadRequestObjectResult(InvalidBody);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
