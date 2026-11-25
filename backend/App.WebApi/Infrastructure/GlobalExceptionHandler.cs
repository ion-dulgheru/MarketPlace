using Microsoft.AspNetCore.Diagnostics;

namespace App.WebApi.Infrastructure;

/// <summary>
/// Single responsibility: coordinate the handling of an unhandled exception
/// (log it, then send a safe response). The decisions are delegated to
/// <see cref="ExceptionStatusCodeMapper"/> and <see cref="SafeProblemDetailsFactory"/>.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string ProblemJsonContentType = "application/problem+json";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        var statusCode = ExceptionStatusCodeMapper.Map(exception);
        LogException(exception, statusCode, httpContext.TraceIdentifier);
        httpContext.Response.StatusCode = statusCode;

        if (statusCode == ExceptionStatusCodeMapper.ClientClosedRequest)
        {
            return true;
        }

        var problem = SafeProblemDetailsFactory.Create(statusCode, httpContext.TraceIdentifier);
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, ProblemJsonContentType, cancellationToken);
        return true;
    }

    private void LogException(Exception exception, int statusCode, string traceId)
    {
        var level = statusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning;
        logger.Log(level, exception, "Unhandled exception. Status={StatusCode} TraceId={TraceId}", statusCode, traceId);
    }
}
