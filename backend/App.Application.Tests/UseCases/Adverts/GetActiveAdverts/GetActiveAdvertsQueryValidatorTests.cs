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
}