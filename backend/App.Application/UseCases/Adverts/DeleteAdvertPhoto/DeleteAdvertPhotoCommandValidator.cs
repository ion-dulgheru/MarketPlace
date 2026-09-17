using App.Domain.Errors;
using FluentValidation;

namespace App.Application.UseCases.Adverts.DeleteAdvertPhoto;

public class DeleteAdvertPhotoCommandValidator : AbstractValidator<DeleteAdvertPhotoCommand>
{
    public DeleteAdvertPhotoCommandValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEmpty().WithMessage(AdvertErrors.InvalidIdentifier.Message);

        RuleFor(x => x.PhotoUuid)
            .NotEmpty().WithMessage(AdvertErrors.InvalidIdentifier.Message);

        RuleFor(x => x.UserUuid)
            .NotEmpty().WithMessage(AdvertErrors.InvalidIdentifier.Message);
    }
}
