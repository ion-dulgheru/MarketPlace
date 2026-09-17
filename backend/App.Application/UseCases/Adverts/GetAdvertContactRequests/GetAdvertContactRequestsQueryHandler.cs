using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.GetAdvertContactRequests;

public class GetAdvertContactRequestsQueryHandler(
    IAdvertRepository advertRepository,
    IContactRequestRepository contactRequestRepository)
    : IQueryHandler<GetAdvertContactRequestsQuery, IReadOnlyList<ContactRequestResponse>>
{
    public async Task<Result<IReadOnlyList<ContactRequestResponse>>> Handle(
        GetAdvertContactRequestsQuery query,
        CancellationToken ct)
    {
        if (query.AdvertUuid == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<ContactRequestResponse>>(AdvertErrors.InvalidIdentifier);
        }

        var advert = await advertRepository.GetByUuidForOwnerAsync(query.AdvertUuid, query.UserUuid, ct);

        if (advert is null)
        {
            return Result.Failure<IReadOnlyList<ContactRequestResponse>>(AdvertErrors.NotFound);
        }

        var contactRequests = await contactRequestRepository.GetByAdvertUuidAsync(advert.Uuid, ct);

        var response = contactRequests
            .Select(cr => new ContactRequestResponse(
                cr.Uuid,
                cr.FromUserUuid,
                cr.Message,
                cr.Status.ToString(),
                cr.CreatedDate))
            .ToList();

        return Result.Success<IReadOnlyList<ContactRequestResponse>>(response);
    }
}