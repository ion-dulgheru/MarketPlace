using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.GetFavoriteAdverts;

public class GetFavoriteAdvertsQueryHandler(IFavoriteAdvertRepository favoriteRepository)
    : IQueryHandler<GetFavoriteAdvertsQuery, GetAdvertsResponse>
{
    public async Task<Result<GetAdvertsResponse>> Handle(
        GetFavoriteAdvertsQuery query,
        CancellationToken ct)
    {
        var (adverts, totalCount) = await favoriteRepository.GetFavoritesByUserAsync(
            query.UserUuid,
            query.Page,
            query.PageSize,
            ct);

        var response = adverts
            .Select(advert => new AdvertResponse(
                advert.Guid,
                advert.Title,
                advert.Description,
                advert.Price,
                advert.SurfaceArea,
                advert.Rooms,
                advert.Floor,
                advert.Status.ToString(),
                advert.Type.ToString(),
                advert.CreatedDate,
                new AddressResponse(
                    advert.Address.Country,
                    advert.Address.City,
                    advert.Address.Region,
                    advert.Address.StreetAddress,
                    advert.Address.StreetNumber),
                advert.Photos
                    .OrderByDescending(p => p.IsPrimary)
                    .Take(1)
                    .Select(p => new AdvertPhotoResponse(p.Guid, p.PhotoUrl, p.IsPrimary))
                    .ToList(),
                advert.BuildingType.ToString(),
                advert.Levels,
                advert.ApartmentFloor,
                advert.ApartmentNumber,
                advert.ApartmentBlock,
                advert.GardenSquareMeters,
                advert.UserUuid))
            .ToList();

        return Result.Success(new GetAdvertsResponse(response, query.Page, query.PageSize, totalCount));
    }
}
