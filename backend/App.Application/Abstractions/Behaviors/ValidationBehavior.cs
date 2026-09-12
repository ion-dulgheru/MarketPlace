using System.Collections.Concurrent;
using System.Linq.Expressions;
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

        return ValidationResultFactory.Create(error);
    }

    private static class ValidationResultFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<Error, TResponse>> Cache = new();

        public static TResponse Create(Error error)
        {
            var factory = Cache.GetOrAdd(typeof(TResponse), type =>
            {
                if (type == typeof(Result))
                {
                    return err => (TResponse)(object)Result.Failure(err);
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var valueType = type.GetGenericArguments()[0];
                    var method = typeof(Result)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
                        .MakeGenericMethod(valueType);

                    var errorParam = Expression.Parameter(typeof(Error), "error");
                    var call = Expression.Call(method, errorParam);
                    return Expression.Lambda<Func<Error, TResponse>>(call, errorParam).Compile();
                }

                throw new InvalidOperationException($"Type {type.Name} is not a valid Result response type.");
            });

            return factory(error);
        }
    }
}

