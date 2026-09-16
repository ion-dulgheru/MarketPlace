using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Adverts.GetAdvertById;

public record GetAdvertByIdQuery(Guid AdvertUuid) : IQuery<AdvertResponse>;
