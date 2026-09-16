using App.Application.UseCases.Adverts.GetActiveAdverts;
using App.Contracts.Requests.Adverts;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetActiveAdverts;

public class GetActiveAdvertsCommandValidatorTests
{
    private readonly GetActiveAdvertsCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenPageSizeExceedsMaximum_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(1, 101)));

        result.ShouldHaveValidationErrorFor(x => x.Request.PageSize);
    }

    [Fact]
    public void Validate_WhenPaginationIsValid_ShouldNotHaveValidationErrors()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(1, 100)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenMaxPriceLessThanMinPrice_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(MinPrice: 10000m, MaxPrice: 5000m)));

        result.ShouldHaveValidationErrorFor(x => x.Request.MaxPrice);
    }

    [Fact]
    public void Validate_WhenMaxSurfaceAreaLessThanMinSurfaceArea_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(MinSurfaceArea: 100m, MaxSurfaceArea: 50m)));

        result.ShouldHaveValidationErrorFor(x => x.Request.MaxSurfaceArea);
    }

    [Fact]
    public void Validate_WhenTypeIsInvalid_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(Type: "InvalidType")));

        result.ShouldHaveValidationErrorFor(x => x.Request.Type);
    }

    [Fact]
    public void Validate_WhenSortByIsInvalid_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(SortBy: "unsupportedField")));

        result.ShouldHaveValidationErrorFor(x => x.Request.SortBy);
    }

    [Fact]
    public void Validate_WhenAllFiltersAreValid_ShouldNotHaveValidationErrors()
    {
        var result = _validator.TestValidate(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(
                Page: 1,
                PageSize: 20,
                SearchTerm: "villa",
                Type: "Sale",
                MinPrice: 1000m,
                MaxPrice: 50000m,
                MinSurfaceArea: 50m,
                MaxSurfaceArea: 200m,
                Rooms: 3,
                SortBy: "price",
                SortDescending: false)));

        result.ShouldNotHaveAnyValidationErrors();
    }
}