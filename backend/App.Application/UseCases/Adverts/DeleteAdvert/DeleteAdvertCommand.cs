using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.DeleteAdvert;

public record DeleteAdvertCommand(Guid AdvertUuid, Guid UserUuid) : ICommand;