using System.Reflection;
using App.Domain.Shared;
using FluentValidation;
using MediatR;

namespace App.Application.Abstractions.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));
        var errors = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (errors.Count == 0)
        {
            return await next();
        }

        var error = Error.Validation(
            "Validation.Failed",
            string.Join("; ", errors.Select(e => e.ErrorMessage)));

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var responseType = typeof(TResponse);
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Type {responseType.Name} is not a valid Result response type.");
        }

        var failureMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(Result.Failure) &&
                method.IsGenericMethodDefinition &&
                method.GetParameters().Length == 1)
            .MakeGenericMethod(responseType.GetGenericArguments()[0]);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}

