using FluentValidation;

namespace App.Application.UseCases.Adverts.GetMyAdverts;

public class GetMyAdvertsCommandValidator : AbstractValidator<GetMyAdvertsCommand>
{
    private const int MaxPageSize = 100;

    public GetMyAdvertsCommandValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0);

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}