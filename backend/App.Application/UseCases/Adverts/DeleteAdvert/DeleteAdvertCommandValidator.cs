using FluentValidation;

namespace App.Application.UseCases.Adverts.DeleteAdvert;

public class DeleteAdvertCommandValidator : AbstractValidator<DeleteAdvertCommand>
{
    public DeleteAdvertCommandValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.UserUuid)
            .NotEqual(Guid.Empty);
    }
}