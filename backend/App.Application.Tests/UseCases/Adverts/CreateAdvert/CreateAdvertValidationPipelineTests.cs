using App.Application.Abstractions.Behaviors;
using App.Application.UseCases.Adverts.CreateAdvert;
using App.Contracts.Requests.Adverts;
using App.Domain.Shared;
using MediatR;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.CreateAdvert;

public class CreateAdvertValidationPipelineTests
{
    [Fact]
    public async Task Handle_WhenRequestHasValidationErrors_ShortCircuitsPipelineAndReturnsValidationFailure()
    {
        // 1. Arrange
        var validator = new CreateAdvertCommandValidator();
        var behavior = new ValidationBehavior<CreateAdvertCommand, Result<Guid>>([validator]);
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();

        var invalidRequest = new CreateAdvertRequest(
            "", // Empty title triggers FluentValidation
            "Some description",
            100000m,
            50m,
            2,
            1,
            "Sale");
        var command = new CreateAdvertCommand(invalidRequest, Guid.NewGuid());

        // 2. Act
        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Validation.Failed", result.Error.Code);
        Assert.Contains("Title is required.", result.Error.Message);

        // Ensure next handler in pipeline was NEVER executed
        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_PassesThroughToNextHandler()
    {
        // 1. Arrange
        var validator = new CreateAdvertCommandValidator();
        var behavior = new ValidationBehavior<CreateAdvertCommand, Result<Guid>>([validator]);
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();
        var expectedGuid = Guid.NewGuid();
        nextMock.Setup(n => n(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(expectedGuid));

        var validRequest = new CreateAdvertRequest(
            "Spacious Flat",
            "Great views and location",
            150000m,
            75m,
            3,
            2,
            "Sale");
        var command = new CreateAdvertCommand(validRequest, Guid.NewGuid());

        // 2. Act
        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedGuid, result.Value);

        // Ensure next handler was called exactly once
        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Once);
    }
}

