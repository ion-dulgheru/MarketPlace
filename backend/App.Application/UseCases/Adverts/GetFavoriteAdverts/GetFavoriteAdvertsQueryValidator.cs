using FluentValidation;

namespace App.Application.UseCases.Adverts.GetFavoriteAdverts;

public class GetFavoriteAdvertsQueryValidator : AbstractValidator<GetFavoriteAdvertsQuery>
{
    private const int MaxPageSize = 100;

    public GetFavoriteAdvertsQueryValidator()
    {
        RuleFor(x => x.UserUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}
