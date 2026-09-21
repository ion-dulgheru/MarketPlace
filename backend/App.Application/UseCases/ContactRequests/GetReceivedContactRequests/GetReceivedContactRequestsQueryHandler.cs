using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.ContactRequests.GetReceivedContactRequests;

public class GetReceivedContactRequestsQueryHandler(
    IContactRequestRepository contactRequestRepository,
    IUserRepository? userRepository = null)
    : IQueryHandler<GetReceivedContactRequestsQuery, IReadOnlyList<ContactRequestResponse>>
{
    public async Task<Result<IReadOnlyList<ContactRequestResponse>>> Handle(
        GetReceivedContactRequestsQuery query,
        CancellationToken ct)
    {
        var requestsWithTitles = await contactRequestRepository.GetReceivedByOwnerUserUuidAsync(query.UserUuid, ct);

        var response = new List<ContactRequestResponse>();

        foreach (var (cr, advertTitle) in requestsWithTitles)
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
                senderPhone,
                cr.AdvertUuid,
                advertTitle));
        }

        return Result.Success<IReadOnlyList<ContactRequestResponse>>(response);
    }
}
