using App.Domain.Errors;
using FluentValidation;

namespace App.Application.UseCases.Adverts.SendContactRequest;

public class SendContactRequestCommandValidator : AbstractValidator<SendContactRequestCommand>
{
    public SendContactRequestCommandValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(ContactRequestErrors.MessageRequired.Message)
            .MaximumLength(1000).WithMessage(ContactRequestErrors.MessageTooLong.Message);
    }
}