using System.Diagnostics;
using App.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace App.Application.Abstractions.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();

            if (response is Result { IsFailure: true } result)
            {
                logger.LogWarning(
                    "{RequestName} Failed after {ElapsedMilliseconds}ms - {ErrorCode}: {ErrorMessage}",
                    requestName, stopwatch.ElapsedMilliseconds, result.Error.Code, result.Error.Message);
            }
            else
            {
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
            }
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                "Unhandled expection in {RequestName} after {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}