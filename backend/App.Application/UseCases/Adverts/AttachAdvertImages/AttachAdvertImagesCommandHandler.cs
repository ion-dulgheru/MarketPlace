using App.Application.Abstractions;
using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.AttachAdvertImages;

public class AttachAdvertImagesCommandHandler(
    IAdvertRepository advertRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    IAdminAccessService adminAccessService)
    : ICommandHandler<AttachAdvertImagesCommand, AttachAdvertImagesResponse>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

    public async Task<Result<AttachAdvertImagesResponse>> Handle(AttachAdvertImagesCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByUuidAsync(command.UserUuid, ct);
        if (user is null || !adminAccessService.IsAdmin(user.Email))
        {
            return Result.Failure<AttachAdvertImagesResponse>(AdminErrors.Forbidden);
        }

        var (adverts, _) = await advertRepository.GetByUserAsync(command.UserUuid, 1, 500, ct);
        var errors = new List<string>();
        var processedCount = 0;

        foreach (var item in command.Items)
        {
            var advert = adverts.FirstOrDefault(a =>
                a.Title == item.Title &&
                a.Price == item.Price &&
                a.SurfaceArea == item.SurfaceArea &&
                a.Floor == item.Floor);

            if (advert is null)
            {
                errors.Add($"No matching advert for '{item.Title}' ({item.Price}, {item.SurfaceArea}mp, floor {item.Floor}).");
                continue;
            }

            foreach (var placeholder in advert.Photos.Where(p => p.PhotoUrl == AdvertImportDefaults.PlaceholderPhotoUrl).ToList())
            {
                advert.RemovePhoto(placeholder.Guid);
            }

            var attachedAny = advert.Photos.Any(p => p.PhotoUrl != AdvertImportDefaults.PlaceholderPhotoUrl);

            foreach (var path in item.ImagePaths)
            {
                var extension = Path.GetExtension(path);
                if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension.ToLowerInvariant()))
                {
                    errors.Add($"'{item.Title}': skipped '{path}' (unsupported extension).");
                    continue;
                }

                if (!File.Exists(path))
                {
                    errors.Add($"'{item.Title}': file not found '{path}'.");
                    continue;
                }

                if (new FileInfo(path).Length > MaxFileSizeInBytes)
                {
                    errors.Add($"'{item.Title}': skipped '{path}' (exceeds 5 MB).");
                    continue;
                }

                await using var stream = File.OpenRead(path);
                var photoUrl = await fileStorageService.SaveFileAsync(stream, Path.GetFileName(path), ct);
                advert.AddPhoto(AdvertPhoto.Create(photoUrl, isPrimary: !attachedAny));
                attachedAny = true;
            }

            if (attachedAny)
            {
                processedCount++;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AttachAdvertImagesResponse(processedCount, errors.Count, errors));
    }
}
