using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.SendContactRequest;

public record SendContactRequestCommand(Guid AdvertUuid, string Message, Guid UserUuid) : ICommand<Guid>;