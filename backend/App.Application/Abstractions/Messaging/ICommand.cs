using App.Domain.Shared;
using MediatR;

namespace App.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;