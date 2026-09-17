using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.GetMyAdverts;

public class GetMyAdvertsCommandHandler(IAdvertRepository advertRepository)
    : IQueryHandler<GetMyAdvertsCommand, GetAdvertsResponse>
{
    public async Task<Result<GetAdvertsResponse>> Handle(
        GetMyAdvertsCommand query,
        CancellationToken ct)
    {
        var adverts = await advertRepository.GetByUserAsync(
            query.UserUuid,
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
                    .Select(p => new AdvertPhotoResponse(p.Guid, p.PhotoUrl, p.IsPrimary))
                    .ToList()))
            .ToList();

        return new GetAdvertsResponse(response, query.Request.Page, query.Request.PageSize);
    }
}