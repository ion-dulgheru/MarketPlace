using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Adverts.GetFavoriteAdverts;

public record GetFavoriteAdvertsQuery(Guid UserUuid, int Page = 1, int PageSize = 20)
    : IQuery<GetAdvertsResponse>;
