using App.Contracts.Requests.Adverts;
using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.CreateAdvert;

public record CreateAdvertCommand(
    CreateAdvertRequest Request,
    Guid UserUuid
) : ICommand<Guid>;