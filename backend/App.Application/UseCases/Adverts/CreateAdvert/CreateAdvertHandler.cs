using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.CreateAdvert;

public class CreateAdvertHandler(
    IAdvertRepository advertRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAdvertCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAdvertCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<AdvertType>(command.Request.Type, ignoreCase: true, out var type))
        {
            return Result.Failure<Guid>(AdvertErrors.InvalidType);
        }

        var advert = new Advert(
            command.UserUuid,
            command.Request.Title,
            command.Request.Description,
            command.Request.Price,
            command.Request.SurfaceArea,
            command.Request.Rooms,
            command.Request.Floor,
            type,
            DateTime.UtcNow.AddDays(30));

        await advertRepository.AddAsync(advert, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(advert.Uuid);
    }
}