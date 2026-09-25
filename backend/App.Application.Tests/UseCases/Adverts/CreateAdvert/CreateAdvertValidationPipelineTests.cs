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
        var validator = new CreateAdvertCommandValidator();
        var behavior = new ValidationBehavior<CreateAdvertCommand, Result<Guid>>([validator]);
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();

        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var invalidRequest = new CreateAdvertRequest(
            "",
            "Some description",
            100000m,
            50m,
            2,
            1,
            "Sale",
            defaultAddress);
        var command = new CreateAdvertCommand(invalidRequest, Guid.NewGuid());

        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Validation.Failed", result.Error.Code);
        Assert.Contains("Title is required.", result.Error.Message);

        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestHasInvalidType_ReturnsValidationFailure()
    {
        var validator = new CreateAdvertCommandValidator();
        var behavior = new ValidationBehavior<CreateAdvertCommand, Result<Guid>>([validator]);
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();

        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var invalidRequest = new CreateAdvertRequest(
            "Apartment",
            "Some description",
            100000m,
            50m,
            2,
            1,
            "InvalidTypeName",
            defaultAddress);
        var command = new CreateAdvertCommand(invalidRequest, Guid.NewGuid());

        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Contains("Advert type must be 'Sale' or 'Rent'.", result.Error.Message);
        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_PassesThroughToNextHandler()
    {
        var validator = new CreateAdvertCommandValidator();
        var behavior = new ValidationBehavior<CreateAdvertCommand, Result<Guid>>([validator]);
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();
        var expectedGuid = Guid.NewGuid();
        nextMock.Setup(n => n(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(expectedGuid));

        var defaultAddress = new AddressRequest("USA", "New York", "NY", "5th Ave", "101");
        var validRequest = new CreateAdvertRequest(
            "Spacious Flat",
            "Great views and location",
            150000m,
            75m,
            3,
            2,
            "Sale",
            defaultAddress);
        var command = new CreateAdvertCommand(validRequest, Guid.NewGuid());

        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedGuid, result.Value);

        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Once);
    }
}

