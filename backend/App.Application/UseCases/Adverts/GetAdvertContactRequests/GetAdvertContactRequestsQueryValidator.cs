using FluentValidation;

namespace App.Application.UseCases.Adverts.GetAdvertContactRequests;

public class GetAdvertContactRequestsQueryValidator : AbstractValidator<GetAdvertContactRequestsQuery>
{
    public GetAdvertContactRequestsQueryValidator()
    {
        RuleFor(x => x.AdvertUuid).NotEqual(Guid.Empty);
    }
}