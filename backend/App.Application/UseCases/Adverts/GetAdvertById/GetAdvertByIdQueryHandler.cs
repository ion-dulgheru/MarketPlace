using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.GetAdvertById;

public class GetAdvertByIdQueryHandler(IAdvertRepository advertRepository)
    : IQueryHandler<GetAdvertByIdQuery, AdvertResponse>
{
    public async Task<Result<AdvertResponse>> Handle(GetAdvertByIdQuery query, CancellationToken ct)
    {
        if (query.AdvertUuid == Guid.Empty)
        {
            return Result.Failure<AdvertResponse>(AdvertErrors.InvalidIdentifier);
        }

        var advert = await advertRepository.GetByUuidAsync(query.AdvertUuid, ct);

        if (advert is null)
        {
            return Result.Failure<AdvertResponse>(AdvertErrors.NotFound);
        }

        var response = new AdvertResponse(
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
                .Select(p => new AdvertPhotoResponse(p.Guid, p.PhotoUrl, p.IsPrimary))
                .ToList());

        return Result.Success(response);
    }
}
