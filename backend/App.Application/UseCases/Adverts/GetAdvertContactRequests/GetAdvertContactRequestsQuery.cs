using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Adverts.GetAdvertContactRequests;

public record GetAdvertContactRequestsQuery(Guid AdvertUuid, Guid UserUuid)
    : IQuery<IReadOnlyList<ContactRequestResponse>>;