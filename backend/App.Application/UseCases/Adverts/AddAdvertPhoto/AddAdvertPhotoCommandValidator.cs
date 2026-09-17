using App.Domain.Errors;
using FluentValidation;

namespace App.Application.UseCases.Adverts.AddAdvertPhoto;

public class AddAdvertPhotoCommandValidator : AbstractValidator<AddAdvertPhotoCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    public AddAdvertPhotoCommandValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEmpty().WithMessage(AdvertErrors.InvalidIdentifier.Message);

        RuleFor(x => x.UserUuid)
            .NotEmpty().WithMessage(AdvertErrors.InvalidIdentifier.Message);

        RuleFor(x => x.FileLength)
            .GreaterThan(0).WithMessage(AdvertErrors.FileRequired.Message)
            .LessThanOrEqualTo(MaxFileSizeInBytes).WithMessage(AdvertErrors.FileTooLarge.Message);

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage(AdvertErrors.FileRequired.Message)
            .Must(fileName =>
            {
                var ext = Path.GetExtension(fileName);
                return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext.ToLowerInvariant());
            })
            .WithMessage(AdvertErrors.InvalidFileFormat.Message);
    }
}
