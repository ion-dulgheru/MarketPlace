using App.Application.Abstractions.Messaging;
using App.Contracts.Requests.Adverts;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Adverts.GetActiveAdverts;

public record GetActiveAdvertsCommand(GetAdvertsRequest Request)
    : IQuery<GetAdvertsResponse>;