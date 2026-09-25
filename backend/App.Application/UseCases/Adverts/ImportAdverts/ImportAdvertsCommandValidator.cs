using FluentValidation;

namespace App.Application.UseCases.Adverts.ImportAdverts;

public class ImportAdvertsCommandValidator : AbstractValidator<ImportAdvertsCommand>
{
    public ImportAdvertsCommandValidator()
    {
        RuleFor(x => x.Adverts)
            .NotEmpty().WithMessage("At least one advert is required.");
    }
}
