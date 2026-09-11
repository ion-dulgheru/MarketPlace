using FluentValidation;

namespace App.Application.UseCases.Adverts.CreateAdvert;

public class CreateAdvertCommandValidator : AbstractValidator<CreateAdvertCommand>
{
    public CreateAdvertCommandValidator()
    {
        // 1. Validate Title
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        // 2. Validate Price (Must be positive!)
        RuleFor(x => x.Request.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        // 3. Validate Surface Area
        RuleFor(x => x.Request.SurfaceArea)
            .GreaterThan(0).WithMessage("Surface area must be greater than 0.");

        // 4. Validate Rooms
        RuleFor(x => x.Request.Rooms)
            .GreaterThan(0).WithMessage("Room count must be at least 1.");

        // 5. Validate Description Length
        RuleFor(x => x.Request.Description)
            .MaximumLength(4000).WithMessage("Description is too long.");
    }
}