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
        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var request = new CreateAdvertRequest(
            "", 
            "Description", 
            100000m, 
            50m, 
            2, 
            1, 
            "Sale",
            defaultAddress);
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_WhenPriceIsZeroOrNegative_ShouldHaveValidationError()
    {
        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var request = new CreateAdvertRequest(
            "Apartment", 
            "Description", 
            0m, 
            50m, 
            2, 
            1, 
            "Sale",
            defaultAddress);
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Price)
              .WithErrorMessage("Price must be greater than 0.");
    }

    [Fact]
    public void Validate_WhenValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var request = new CreateAdvertRequest(
            "Modern Apartment", 
            "Great place to live", 
            120000m, 
            65m, 
            3, 
            2, 
            "Sale",
            defaultAddress);
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenAddressIsNull_ShouldHaveValidationError()
    {
        var request = new CreateAdvertRequest(
            "Modern Apartment", 
            "Great place to live", 
            120000m, 
            65m, 
            3, 
            2, 
            "Sale",
            null!);
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Address)
              .WithErrorMessage("All address fields are required.");
    }

    [Fact]
    public void Validate_WhenBuildingTypeIsInvalid_ShouldHaveValidationError()
    {
        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var request = new CreateAdvertRequest(
            "Apartment",
            "Description",
            100000m,
            50m,
            2,
            1,
            "Sale",
            defaultAddress,
            BuildingType: "Castle");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.BuildingType)
              .WithErrorMessage("Invalid building type. Supported types: Apartment, House.");
    }

    [Fact]
    public void Validate_WhenLevelsIsZeroOrNegative_ShouldHaveValidationError()
    {
        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var request = new CreateAdvertRequest(
            "Apartment",
            "Description",
            100000m,
            50m,
            2,
            1,
            "Sale",
            defaultAddress,
            Levels: 0);
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Levels)
              .WithErrorMessage("Levels must be greater than 0.");
    }
}

