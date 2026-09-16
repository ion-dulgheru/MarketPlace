using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
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
        var adverts = await advertRepository.GetActiveAsync(
            query.Request.Page,
            query.Request.PageSize,
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
                    .Select(p => new AdvertPhotoResponse(p.PhotoUrl, p.IsPrimary))
                    .ToList()))
            .ToList();

        return new GetAdvertsResponse(
            response,
            query.Request.Page,
            query.Request.PageSize);
    }
}