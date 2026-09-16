using App.Domain.Errors;
using FluentValidation;

namespace App.Application.UseCases.Adverts.UpdateAdvert;

public class UpdateAdvertCommandValidator : AbstractValidator<UpdateAdvertCommand>
{
    public UpdateAdvertCommandValidator()
    {
        RuleFor(x => x.AdvertUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.UserUuid)
            .NotEqual(Guid.Empty);

        When(x => x.Request.Title is not null, () =>
        {
            RuleFor(x => x.Request.Title!)
                .NotEmpty().WithMessage(AdvertErrors.TitleRequired.Message)
                .MaximumLength(200).WithMessage(AdvertErrors.TitleTooLong.Message);
        });

        When(x => x.Request.Description is not null, () =>
        {
            RuleFor(x => x.Request.Description!)
                .MaximumLength(4000).WithMessage(AdvertErrors.DescriptionTooLong.Message);
        });

        When(x => x.Request.Price is not null, () =>
        {
            RuleFor(x => x.Request.Price!.Value)
                .GreaterThan(0).WithMessage(AdvertErrors.PriceMustBePositive.Message);
        });

        When(x => x.Request.SurfaceArea is not null, () =>
        {
            RuleFor(x => x.Request.SurfaceArea!.Value)
                .GreaterThan(0).WithMessage(AdvertErrors.SurfaceAreaMustBePositive.Message);
        });

        When(x => x.Request.Rooms is not null, () =>
        {
            RuleFor(x => x.Request.Rooms!.Value)
                .GreaterThan(0).WithMessage(AdvertErrors.RoomsMustBePositive.Message);
        });

        When(x => x.Request.Address is not null, () =>
        {
            RuleFor(x => x.Request.Address!.Country).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.Address!.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.Address!.Region).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.Address!.StreetAddress).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Request.Address!.StreetNumber).NotEmpty().MaximumLength(20);
        });
    }
}