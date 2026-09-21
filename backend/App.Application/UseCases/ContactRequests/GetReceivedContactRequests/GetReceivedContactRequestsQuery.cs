using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.ContactRequests.GetReceivedContactRequests;

public record GetReceivedContactRequestsQuery(Guid UserUuid)
    : IQuery<IReadOnlyList<ContactRequestResponse>>;
