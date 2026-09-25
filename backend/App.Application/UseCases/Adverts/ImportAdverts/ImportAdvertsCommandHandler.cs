using App.Application.Abstractions;
using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;
using App.Domain.ValueObjects;

namespace App.Application.UseCases.Adverts.ImportAdverts;

public class ImportAdvertsCommandHandler(
    IAdvertRepository advertRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IHtmlSanitizerService htmlSanitizerService,
    IAdminAccessService adminAccessService)
    : ICommandHandler<ImportAdvertsCommand, ImportAdvertsResponse>
{
    private const string DefaultCountry = "Moldova";

    public async Task<Result<ImportAdvertsResponse>> Handle(ImportAdvertsCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByUuidAsync(command.UserUuid, ct);
        if (user is null || !adminAccessService.IsAdmin(user.Email))
        {
            return Result.Failure<ImportAdvertsResponse>(AdminErrors.Forbidden);
        }

        var errors = new List<string>();
        var importedCount = 0;

        for (var index = 0; index < command.Adverts.Count; index++)
        {
            var item = command.Adverts[index];

            var addressResult = Address.Create(
                DefaultCountry,
                item.Address.City,
                string.IsNullOrWhiteSpace(item.Address.Sector) ? item.Address.City : item.Address.Sector,
                string.IsNullOrWhiteSpace(item.Address.Street) ? "Nespecificat" : item.Address.Street,
                string.IsNullOrWhiteSpace(item.Address.HouseNumber) ? "-" : item.Address.HouseNumber);

            if (addressResult.IsFailure)
            {
                errors.Add($"Item {index} ('{item.Title}'): {addressResult.Error.Message}");
                continue;
            }

            if (!Enum.TryParse<AdvertType>(item.Type, true, out var type))
            {
                errors.Add($"Item {index} ('{item.Title}'): invalid type '{item.Type}'.");
                continue;
            }

            var sanitizedDescription = htmlSanitizerService.Sanitize(item.Description);

            var advert = Advert.Create(
                command.UserUuid,
                item.Title,
                sanitizedDescription,
                item.Price,
                item.SurfaceArea,
                item.Rooms,
                item.Floor,
                type,
                addressResult.Value,
                DateTime.UtcNow.AddDays(30),
                BuildingType.Apartment,
                item.Levels,
                item.ApartmentFloor,
                item.ApartmentNumber,
                item.ApartmentBlock,
                item.GardenSquareMeters,
                Currency.Eur);

            advert.AddPhoto(AdvertPhoto.Create(AdvertImportDefaults.PlaceholderPhotoUrl, true));

            await advertRepository.AddAsync(advert, ct);
            importedCount++;
        }

        if (importedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Result.Success(new ImportAdvertsResponse(importedCount, errors.Count, errors));
    }
}
