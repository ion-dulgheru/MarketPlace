using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.GetActiveAdverts;

public class GetActiveAdvertsCommandHandler(IAdvertRepository advertRepository)
    : IQueryHandler<GetActiveAdvertsCommand, GetAdvertsResponse>
{
    public async Task<Result<GetAdvertsResponse>> Handle(
        GetActiveAdvertsCommand query,
        CancellationToken ct)
    {
        AdvertType? advertType = null;
        if (!string.IsNullOrWhiteSpace(query.Request.Type) &&
            Enum.TryParse<AdvertType>(query.Request.Type, true, out var parsedType))
        {
            advertType = parsedType;
        }

        var criteria = new AdvertSearchCriteria(
            Page: query.Request.Page,
            PageSize: query.Request.PageSize,
            SearchTerm: query.Request.SearchTerm,
            Type: advertType,
            City: query.Request.City,
            MinPrice: query.Request.MinPrice,
            MaxPrice: query.Request.MaxPrice,
            MinSurfaceArea: query.Request.MinSurfaceArea,
            MaxSurfaceArea: query.Request.MaxSurfaceArea,
            Rooms: query.Request.Rooms,
            SortBy: query.Request.SortBy,
            SortDescending: query.Request.SortDescending);

        var (adverts, totalCount) = await advertRepository.GetActiveAsync(criteria, ct);

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
                advert.GardenSquareMeters))
            .ToList();

        return new GetAdvertsResponse(
            response,
            query.Request.Page,
            query.Request.PageSize,
            totalCount);
    }
}