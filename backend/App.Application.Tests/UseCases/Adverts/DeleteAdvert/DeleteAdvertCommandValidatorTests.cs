using App.Application.UseCases.Adverts.DeleteAdvert;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.DeleteAdvert;

public class DeleteAdvertCommandValidatorTests
{
    private readonly DeleteAdvertCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenAdvertUuidIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteAdvertCommand(Guid.Empty, Guid.NewGuid(), false);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AdvertUuid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        var command = new DeleteAdvertCommand(Guid.NewGuid(), Guid.NewGuid(), false);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}