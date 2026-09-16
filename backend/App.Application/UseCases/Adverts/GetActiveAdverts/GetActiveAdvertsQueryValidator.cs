using App.Domain.Entities;
using App.Domain.Errors;
using FluentValidation;

namespace App.Application.UseCases.Adverts.GetActiveAdverts;

public class GetActiveAdvertsCommandValidator : AbstractValidator<GetActiveAdvertsCommand>
{
    private const int MaxPageSize = 100;
    private static readonly string[] AllowedSortBy = ["date", "price", "surfacearea"];

    public GetActiveAdvertsCommandValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0);

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, MaxPageSize);

        RuleFor(x => x.Request.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.MinPrice.HasValue);

        RuleFor(x => x.Request.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.MaxPrice.HasValue);

        RuleFor(x => x.Request.MaxPrice)
            .GreaterThanOrEqualTo(x => x.Request.MinPrice!.Value)
            .When(x => x.Request.MaxPrice.HasValue && x.Request.MinPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");

        RuleFor(x => x.Request.MinSurfaceArea)
            .GreaterThan(0)
            .When(x => x.Request.MinSurfaceArea.HasValue);

        RuleFor(x => x.Request.MaxSurfaceArea)
            .GreaterThan(0)
            .When(x => x.Request.MaxSurfaceArea.HasValue);

        RuleFor(x => x.Request.MaxSurfaceArea)
            .GreaterThanOrEqualTo(x => x.Request.MinSurfaceArea!.Value)
            .When(x => x.Request.MaxSurfaceArea.HasValue && x.Request.MinSurfaceArea.HasValue)
            .WithMessage("MaxSurfaceArea must be greater than or equal to MinSurfaceArea.");

        RuleFor(x => x.Request.Rooms)
            .GreaterThan(0)
            .When(x => x.Request.Rooms.HasValue);

        RuleFor(x => x.Request.Type)
            .Must(type => Enum.TryParse<AdvertType>(type, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Type))
            .WithMessage(AdvertErrors.InvalidType.Message);

        RuleFor(x => x.Request.SortBy)
            .Must(sortBy => AllowedSortBy.Contains(sortBy!.Trim().ToLower()))
            .When(x => !string.IsNullOrWhiteSpace(x.Request.SortBy))
            .WithMessage("SortBy must be 'date', 'price', or 'surfacearea'.");
    }
}