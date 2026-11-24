using App.Application.Abstractions.Messaging;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.ContactRequests.MarkContactRequestAsRead;

public class MarkContactRequestAsReadCommandHandler(
    IContactRequestRepository contactRequestRepository,
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MarkContactRequestAsReadCommand>
{
    public async Task<Result> Handle(MarkContactRequestAsReadCommand command, CancellationToken ct)
    {
        if (command.ContactRequestUuid == Guid.Empty)
        {
            return Result.Failure(ContactRequestErrors.InvalidIdentifier);
        }

        var contactRequest = await contactRequestRepository.GetByUuidAsync(command.ContactRequestUuid, ct);

        if (contactRequest is null)
        {
            return Result.Failure(ContactRequestErrors.NotFound);
        }

        var isSender = command.UserUuid == contactRequest.FromUserUuid;
        var isOwner = false;

        if (!isSender)
        {
            var advert = await advertRepository.GetByUuidForOwnerAsync(contactRequest.AdvertUuid, command.UserUuid, ct);
            isOwner = advert is not null;
        }

        if (!isSender && !isOwner)
        {
            return Result.Failure(ContactRequestErrors.NotFound);
        }

        contactRequest.MarkAsRead();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}