using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Adverts.AddAdvertPhoto;

public record AddAdvertPhotoCommand(
    Guid AdvertUuid,
    Guid UserUuid,
    Stream FileStream,
    string FileName,
    long FileLength,
    bool IsPrimary
) : ICommand<AdvertPhotoResponse>;
