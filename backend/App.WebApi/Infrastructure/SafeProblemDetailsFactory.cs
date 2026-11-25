using Microsoft.AspNetCore.Mvc;

namespace App.WebApi.Infrastructure;

/// <summary>
/// Single responsibility: build the error body sent to the client.
/// It never contains the exception message or stack trace, only a generic title and the traceId.
/// </summary>
public static class SafeProblemDetailsFactory
{
    public static ProblemDetails Create(int statusCode, string traceId) => new()
    {
        Status = statusCode,
        Title = GetTitle(statusCode),
        Extensions = { ["traceId"] = traceId }
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status401Unauthorized => "Authentication is required.",
        StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
        _ => "Request could not be processed."
    };
}
