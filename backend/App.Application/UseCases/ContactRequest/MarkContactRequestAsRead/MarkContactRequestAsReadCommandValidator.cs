using FluentValidation;

namespace App.Application.UseCases.ContactRequests.MarkContactRequestAsRead;

public class MarkContactRequestAsReadCommandValidator : AbstractValidator<MarkContactRequestAsReadCommand>
{
    public MarkContactRequestAsReadCommandValidator()
    {
        RuleFor(x => x.ContactRequestUuid).NotEqual(Guid.Empty);
    }
}