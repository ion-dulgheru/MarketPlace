using App.Application.Abstractions.Messaging;
using App.Contracts.Requests.Adverts;

namespace App.Application.UseCases.Adverts.UpdateAdvertStatus;

public record UpdateAdvertStatusCommand(
    Guid AdvertUuid,
    UpdateAdvertStatusRequest Request,
    Guid UserUuid
) : ICommand;