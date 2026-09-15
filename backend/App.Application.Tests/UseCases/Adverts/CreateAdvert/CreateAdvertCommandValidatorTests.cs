using App.Application.UseCases.Adverts.CreateAdvert;
using App.Contracts.Requests.Adverts;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.CreateAdvert;

public class CreateAdvertCommandValidatorTests
{
    private readonly CreateAdvertCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenTitleIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateAdvertRequest(
            "", 
            "Description", 
            100000m, 
            50m, 
            2, 
            1, 
            "Sale");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_WhenPriceIsZeroOrNegative_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateAdvertRequest(
            "Apartment", 
            "Description", 
            0m, 
            50m, 
            2, 
            1, 
            "Sale");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Price)
              .WithErrorMessage("Price must be greater than 0.");
    }

    [Fact]
    public void Validate_WhenValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = new CreateAdvertRequest(
            "Modern Apartment", 
            "Great place to live", 
            120000m, 
            65m, 
            3, 
            2, 
            "Sale");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

