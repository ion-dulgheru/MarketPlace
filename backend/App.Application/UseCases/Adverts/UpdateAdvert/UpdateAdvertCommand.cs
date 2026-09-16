using App.Application.Abstractions.Messaging;
using App.Contracts.Requests.Adverts;

namespace App.Application.UseCases.Adverts.UpdateAdvert;

public record UpdateAdvertCommand(
    Guid AdvertUuid,
    UpdateAdvertRequest Request,
    Guid UserUuid
) : ICommand;