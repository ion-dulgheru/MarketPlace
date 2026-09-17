using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.AddAdvertPhoto;

public class AddAdvertPhotoCommandHandler(
    IAdvertRepository advertRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddAdvertPhotoCommand, AdvertPhotoResponse>
{
    public async Task<Result<AdvertPhotoResponse>> Handle(AddAdvertPhotoCommand command, CancellationToken ct)
    {
        var advert = await advertRepository.GetByUuidForOwnerAsync(command.AdvertUuid, command.UserUuid, ct);
        if (advert is null)
        {
            return Result.Failure<AdvertPhotoResponse>(AdvertErrors.NotFound);
        }

        var photoUrl = await fileStorageService.SaveFileAsync(
            command.FileStream,
            command.FileName,
            ct);

        var photo = AdvertPhoto.Create(photoUrl, command.IsPrimary);
        advert.AddPhoto(photo);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AdvertPhotoResponse(photo.Guid, photo.PhotoUrl, photo.IsPrimary));
    }
}
