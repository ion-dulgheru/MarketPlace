using App.Application.UseCases.Adverts.UpdateAdvertStatus;
using App.Contracts.Requests.Adverts;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.UpdateAdvertStatus;

public class UpdateAdvertStatusCommandValidatorTests
{
    private readonly UpdateAdvertStatusCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenStatusIsInvalid_ShouldHaveValidationError()
    {
        var command = new UpdateAdvertStatusCommand(
            Guid.NewGuid(),
            new UpdateAdvertStatusRequest("Archived"),
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Status);
    }
}