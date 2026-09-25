using FluentValidation;

namespace App.Application.UseCases.Adverts.AttachAdvertImages;

public class AttachAdvertImagesCommandValidator : AbstractValidator<AttachAdvertImagesCommand>
{
    public AttachAdvertImagesCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Title).NotEmpty();
            item.RuleFor(x => x.ImagePaths).NotEmpty();
        });
    }
}
