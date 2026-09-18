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

        RuleFor(x => x.Request.BuildingType)
            .NotEmpty()
            .Must(bType => Enum.TryParse<BuildingType>(bType, true, out _))
            .WithMessage("Invalid building type. Supported types: Apartment, House.");

        RuleFor(x => x.Request.Levels)
            .GreaterThan(0).WithMessage("Levels must be greater than 0.");

        When(x => x.Request.ApartmentFloor.HasValue, () =>
        {
            RuleFor(x => x.Request.ApartmentFloor!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Apartment floor must be 0 or greater.");
        });

        When(x => x.Request.GardenSquareMeters.HasValue, () =>
        {
            RuleFor(x => x.Request.GardenSquareMeters!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Garden square meters must be 0 or greater.");
        });

        RuleFor(x => x.Request.Price)
            .GreaterThan(0).WithMessage(AdvertErrors.PriceMustBePositive.Message);

        RuleFor(x => x.Request.SurfaceArea)
            .GreaterThan(0).WithMessage(AdvertErrors.SurfaceAreaMustBePositive.Message);

        RuleFor(x => x.Request.Rooms)
            .GreaterThan(0).WithMessage(AdvertErrors.RoomsMustBePositive.Message);

        RuleFor(x => x.Request.Description)
            .MaximumLength(4000).WithMessage(AdvertErrors.DescriptionTooLong.Message);

        RuleFor(x => x.Request.Address)
            .NotNull().WithMessage(AddressErrors.RequiredField.Message);

        When(x => x.Request.Address is not null, () =>
        {
            RuleFor(x => x.Request.Address.Country)
                .NotEmpty().WithMessage(AddressErrors.RequiredField.Message)
                .MaximumLength(100);

            RuleFor(x => x.Request.Address.City)
                .NotEmpty().WithMessage(AddressErrors.RequiredField.Message)
                .MaximumLength(100);

            RuleFor(x => x.Request.Address.Region)
                .NotEmpty().WithMessage(AddressErrors.RequiredField.Message)
                .MaximumLength(100);

            RuleFor(x => x.Request.Address.StreetAddress)
                .NotEmpty().WithMessage(AddressErrors.RequiredField.Message)
                .MaximumLength(200);

            RuleFor(x => x.Request.Address.StreetNumber)
                .NotEmpty().WithMessage(AddressErrors.RequiredField.Message)
                .MaximumLength(20);
        });
    }
}