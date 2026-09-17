using FluentValidation;

namespace App.Application.UseCases.Adverts.FavoriteAdvert;

public class FavoriteAdvertCommandValidator : AbstractValidator<FavoriteAdvertCommand>
{
    public FavoriteAdvertCommandValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.UserUuid)
            .NotEqual(Guid.Empty);
    }
}
