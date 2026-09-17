using App.Application.UseCases.Adverts.DeleteAdvertPhoto;
using App.Domain.Errors;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.DeleteAdvertPhoto;

public class DeleteAdvertPhotoCommandValidatorTests
{
    private readonly DeleteAdvertPhotoCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        var command = new DeleteAdvertPhotoCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAdvertUuidIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteAdvertPhotoCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.InvalidIdentifier.Message);
    }

    [Fact]
    public void Validate_WhenPhotoUuidIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteAdvertPhotoCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.InvalidIdentifier.Message);
    }

    [Fact]
    public void Validate_WhenUserUuidIsEmpty_ShouldHaveValidationError()
    {
        var command = new DeleteAdvertPhotoCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.InvalidIdentifier.Message);
    }
}
