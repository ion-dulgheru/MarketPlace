using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.FavoriteAdvert;

public class FavoriteAdvertCommandHandler(
    IAdvertRepository advertRepository,
    IFavoriteAdvertRepository favoriteRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<FavoriteAdvertCommand>
{
    public async Task<Result> Handle(FavoriteAdvertCommand command, CancellationToken ct)
    {
        if (command.AdvertUuid == Guid.Empty || command.UserUuid == Guid.Empty)
        {
            return Result.Failure(AdvertErrors.InvalidIdentifier);
        }

        var advert = await advertRepository.GetByUuidAsync(command.AdvertUuid, ct);
        if (advert is null || !advert.IsActive || advert.Status != AdvertStatus.Active)
        {
            return Result.Failure(AdvertErrors.NotFound);
        }

        if (command.IsFavorite)
        {
            if (advert.UserUuid == command.UserUuid)
            {
                return Result.Failure(AdvertErrors.CannotFavoriteOwnAdvert);
            }

            var isAlreadyFavorited = await favoriteRepository.ExistsAsync(command.UserUuid, advert.Id, ct);
            if (isAlreadyFavorited)
            {
                return Result.Success();
            }

            var favorite = App.Domain.Entities.FavoriteAdvert.Create(command.UserUuid, advert.Id);
            await favoriteRepository.AddAsync(favorite, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        else
        {
            var favorite = await favoriteRepository.GetAsync(command.UserUuid, advert.Id, ct);
            if (favorite is not null)
            {
                favoriteRepository.Remove(favorite);
                await unitOfWork.SaveChangesAsync(ct);
            }

            return Result.Success();
        }
    }
}
