using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.DeleteAdvertPhoto;

public record DeleteAdvertPhotoCommand(
    Guid AdvertUuid,
    Guid PhotoUuid,
    Guid UserUuid
) : ICommand;
