using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.Messaging;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.DeleteAdvertPhoto;

public class DeleteAdvertPhotoCommandHandler(
    IAdvertRepository advertRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAdvertPhotoCommand>
{
    public async Task<Result> Handle(DeleteAdvertPhotoCommand command, CancellationToken ct)
    {
        var advert = await advertRepository.GetByUuidForOwnerAsync(command.AdvertUuid, command.UserUuid, ct);
        if (advert is null)
        {
            return Result.Failure(AdvertErrors.NotFound);
        }

        var photo = advert.Photos.FirstOrDefault(p => p.Guid == command.PhotoUuid);
        if (photo is null)
        {
            return Result.Failure(AdvertErrors.PhotoNotFound);
        }

        var removed = advert.RemovePhoto(command.PhotoUuid);
        if (!removed)
        {
            return Result.Failure(AdvertErrors.PhotoNotFound);
        }

        await unitOfWork.SaveChangesAsync(ct);
        await fileStorageService.DeleteFileAsync(photo.PhotoUrl, ct);

        return Result.Success();
    }
}
