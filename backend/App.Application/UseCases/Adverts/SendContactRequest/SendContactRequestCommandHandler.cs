using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.SendContactRequest;

public class SendContactRequestCommandHandler(
    IAdvertRepository advertRepository,
    IContactRequestRepository contactRequestRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SendContactRequestCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SendContactRequestCommand command, CancellationToken ct)
    {
        if (command.AdvertUuid == Guid.Empty)
        {
            return Result.Failure<Guid>(AdvertErrors.InvalidIdentifier);
        }

        var advert = await advertRepository.GetByUuidAsync(command.AdvertUuid, ct);

        if (advert is null)
        {
            return Result.Failure<Guid>(AdvertErrors.NotFound);
        }

        if (advert.Status != AdvertStatus.Active)
        {
            return Result.Failure<Guid>(AdvertErrors.NotActive);
        }

        if (advert.UserUuid == command.UserUuid)
        {
            return Result.Failure<Guid>(AdvertErrors.CannotContactOwnAdvert);
        }

        var contactRequest = ContactRequest.Create(advert.Uuid, command.UserUuid, command.Message);

        await contactRequestRepository.AddAsync(contactRequest, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(contactRequest.Uuid);
    }
}