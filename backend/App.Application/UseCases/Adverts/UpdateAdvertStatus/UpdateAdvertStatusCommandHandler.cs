using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.UpdateAdvertStatus;

public class UpdateAdvertStatusCommandHandler(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAdvertStatusCommand>
{
    public async Task<Result> Handle(UpdateAdvertStatusCommand command, CancellationToken ct)
    {
        if (command.AdvertUuid == Guid.Empty || command.UserUuid == Guid.Empty)
        {
            return Result.Failure(AdvertErrors.InvalidIdentifier);
        }

        var advert = await advertRepository.GetByUuidForOwnerAsync(
            command.AdvertUuid,
            command.UserUuid,
            ct);

        if (advert is null)
        {
            return Result.Failure(AdvertErrors.NotFound);
        }

        if (!Enum.TryParse<AdvertStatus>(command.Request.Status, true, out var status))
        {
            return Result.Failure(AdvertErrors.InvalidStatus);
        }

        advert.ChangeStatus(status);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}