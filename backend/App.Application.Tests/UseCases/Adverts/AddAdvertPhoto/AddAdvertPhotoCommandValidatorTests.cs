using App.Application.UseCases.Adverts.AddAdvertPhoto;
using App.Domain.Errors;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.AddAdvertPhoto;

public class AddAdvertPhotoCommandValidatorTests
{
    private readonly AddAdvertPhotoCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveErrors()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new AddAdvertPhotoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            stream,
            "house.png",
            stream.Length,
            false);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAdvertUuidIsEmpty_ShouldHaveValidationError()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new AddAdvertPhotoCommand(
            Guid.Empty,
            Guid.NewGuid(),
            stream,
            "house.jpg",
            stream.Length,
            false);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.InvalidIdentifier.Message);
    }

    [Fact]
    public void Validate_WhenFileLengthIsZero_ShouldHaveValidationError()
    {
        using var stream = new MemoryStream();
        var command = new AddAdvertPhotoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            stream,
            "house.jpg",
            0,
            false);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.FileRequired.Message);
    }

    [Fact]
    public void Validate_WhenFileExceeds5MB_ShouldHaveValidationError()
    {
        using var stream = new MemoryStream();
        var command = new AddAdvertPhotoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            stream,
            "huge_photo.jpg",
            6 * 1024 * 1024,
            false);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.FileTooLarge.Message);
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("script.exe")]
    [InlineData("image.gif")]
    [InlineData("photo")]
    public void Validate_WhenUnsupportedExtension_ShouldHaveValidationError(string fileName)
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new AddAdvertPhotoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            stream,
            fileName,
            stream.Length,
            false);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == AdvertErrors.InvalidFileFormat.Message);
    }
}
