using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.GetAdvertContactRequests;

public class GetAdvertContactRequestsQueryHandler(
    IAdvertRepository advertRepository,
    IContactRequestRepository contactRequestRepository,
    IUserRepository? userRepository = null)
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

        var response = new List<ContactRequestResponse>();

        foreach (var cr in contactRequests)
        {
            string? senderEmail = null;
            string? senderName = null;
            string? senderPhone = null;

            if (userRepository is not null)
            {
                var user = await userRepository.GetByUuidAsync(cr.FromUserUuid, ct);
                if (user is not null)
                {
                    senderEmail = user.Email;
                    var details = await userRepository.GetDetailsByUserIdAsync(user.Id, ct);
                    if (details is not null)
                    {
                        var fullName = $"{details.FirstName} {details.LastName}".Trim();
                        senderName = string.IsNullOrWhiteSpace(fullName) ? null : fullName;
                        senderPhone = details.PhoneNumber;
                    }
                }
            }

            response.Add(new ContactRequestResponse(
                cr.Uuid,
                cr.FromUserUuid,
                cr.Message,
                cr.Status.ToString(),
                cr.CreatedDate,
                senderName,
                senderEmail,
                senderPhone));
        }

        return Result.Success<IReadOnlyList<ContactRequestResponse>>(response);
    }
}