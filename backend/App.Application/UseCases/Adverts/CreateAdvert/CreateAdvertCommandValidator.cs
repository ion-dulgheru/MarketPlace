using App.Domain.Entities;
using App.Domain.Errors;
using FluentValidation;

namespace App.Application.UseCases.Adverts.CreateAdvert;

public class CreateAdvertCommandValidator : AbstractValidator<CreateAdvertCommand>
{
    public CreateAdvertCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage(AdvertErrors.TitleRequired.Message)
            .MaximumLength(200).WithMessage(AdvertErrors.TitleTooLong.Message);

        RuleFor(x => x.Request.Type)
            .NotEmpty()
            .Must(type => Enum.TryParse<AdvertType>(type, true, out _))
            .WithMessage(AdvertErrors.InvalidType.Message);

        RuleFor(x => x.Request.Price)
            .GreaterThan(0).WithMessage(AdvertErrors.PriceMustBePositive.Message);

        RuleFor(x => x.Request.SurfaceArea)
            .GreaterThan(0).WithMessage(AdvertErrors.SurfaceAreaMustBePositive.Message);

        RuleFor(x => x.Request.Rooms)
            .GreaterThan(0).WithMessage(AdvertErrors.RoomsMustBePositive.Message);

        RuleFor(x => x.Request.Description)
            .MaximumLength(4000).WithMessage(AdvertErrors.DescriptionTooLong.Message);
    }
}