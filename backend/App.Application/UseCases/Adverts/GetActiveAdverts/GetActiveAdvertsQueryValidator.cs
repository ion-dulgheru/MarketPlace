using FluentValidation;

namespace App.Application.UseCases.Adverts.GetActiveAdverts;

public class GetActiveAdvertsCommandValidator : AbstractValidator<GetActiveAdvertsCommand>
{
    private const int MaxPageSize = 100;

    public GetActiveAdvertsCommandValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0);

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}