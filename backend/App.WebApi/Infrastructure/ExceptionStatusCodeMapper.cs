namespace App.WebApi.Infrastructure;

/// <summary>
/// Single responsibility: decide which HTTP status code an unhandled exception becomes.
/// </summary>
public static class ExceptionStatusCodeMapper
{
    /// <summary>Non-standard code (nginx convention): the client closed the connection.</summary>
    public const int ClientClosedRequest = 499;

    public static int Map(Exception exception) => exception switch
    {
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        OperationCanceledException => ClientClosedRequest,
        _ => StatusCodes.Status500InternalServerError
    };
}
