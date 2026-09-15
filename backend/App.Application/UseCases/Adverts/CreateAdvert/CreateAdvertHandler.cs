using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;
using App.Domain.ValueObjects;

namespace App.Application.UseCases.Adverts.CreateAdvert;

public class CreateAdvertHandler(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAdvertCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAdvertCommand command, CancellationToken ct)
    {
        var addressResult = Address.Create(
            command.Request.Address.Country,
            command.Request.Address.City,
            command.Request.Address.Region,
            command.Request.Address.StreetAddress,
            command.Request.Address.StreetNumber);

        if (addressResult.IsFailure)
        {
            return Result.Failure<Guid>(addressResult.Error);
        }

        var type = Enum.Parse<AdvertType>(command.Request.Type, ignoreCase: true);

        var advert = Advert.Create(
            command.UserUuid,
            command.Request.Title,
            command.Request.Description,
            command.Request.Price,
            command.Request.SurfaceArea,
            command.Request.Rooms,
            command.Request.Floor,
            type,
            addressResult.Value,
            DateTime.UtcNow.AddDays(30));

        await advertRepository.AddAsync(advert, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(advert.Uuid);
    }
}