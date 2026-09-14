using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.DeleteAdvert;

public class DeleteAdvertCommandHandler(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAdvertCommand>
{
    public async Task<Result> Handle(DeleteAdvertCommand command, CancellationToken ct)
    {
        var advert = await advertRepository.GetByUuidForOwnerAsync(
            command.AdvertUuid,
            command.UserUuid,
            ct);

        if (advert is null)
        {
            return Result.Failure(AdvertErrors.NotFound);
        }

        advert.SoftDelete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}