using App.Domain.Entities;
using FluentValidation;

namespace App.Application.UseCases.Adverts.UpdateAdvertStatus;

public class UpdateAdvertStatusCommandValidator : AbstractValidator<UpdateAdvertStatusCommand>
{
    public UpdateAdvertStatusCommandValidator()
    {
        RuleFor(x => x.Request.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<AdvertStatus>(status, true, out _))
            .WithMessage("Advert status must be 'Active', 'Sold' or 'Rented'.");
    }
}