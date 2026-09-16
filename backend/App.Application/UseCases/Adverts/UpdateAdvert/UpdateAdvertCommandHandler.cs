using App.Application.Abstractions.Messaging;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;
using App.Domain.ValueObjects;

namespace App.Application.UseCases.Adverts.UpdateAdvert;

public class UpdateAdvertCommandHandler(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAdvertCommand>
{
    public async Task<Result> Handle(UpdateAdvertCommand command, CancellationToken ct)
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

        var address = advert.Address;

        if (command.Request.Address is not null)
        {
            var addressResult = Address.Create(
                command.Request.Address.Country,
                command.Request.Address.City,
                command.Request.Address.Region,
                command.Request.Address.StreetAddress,
                command.Request.Address.StreetNumber);

            if (addressResult.IsFailure)
            {
                return Result.Failure(addressResult.Error);
            }

            address = addressResult.Value;
        }

        advert.Update(
            command.Request.Title ?? advert.Title,
            command.Request.Description ?? advert.Description,
            command.Request.Price ?? advert.Price,
            command.Request.SurfaceArea ?? advert.SurfaceArea,
            command.Request.Rooms ?? advert.Rooms,
            command.Request.Floor ?? advert.Floor,
            address);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}