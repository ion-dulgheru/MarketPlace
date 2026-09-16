using App.Application.Abstractions.Messaging;
using App.Contracts.Requests.Adverts;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Adverts.GetMyAdverts;

public record GetMyAdvertsCommand(GetAdvertsRequest Request, Guid UserUuid)
    : IQuery<GetAdvertsResponse>;