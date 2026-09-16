using FluentValidation;

namespace App.Application.UseCases.Adverts.GetAdvertById;

public class GetAdvertByIdQueryValidator : AbstractValidator<GetAdvertByIdQuery>
{
    public GetAdvertByIdQueryValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEqual(Guid.Empty);
    }
}
