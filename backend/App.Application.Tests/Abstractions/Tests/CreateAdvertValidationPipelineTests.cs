using App.Application.Abstractions.Behaviors;
using App.Application.UseCases.Adverts.CreateAdvert;
using App.Contracts.Requests.Adverts;
using App.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Application.Tests.Abstractions.Behaviors;

public class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNextSucceeds_LogsInformationAndReturnsResponse()
    {
        // 1. Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<CreateAdvertCommand, Result<Guid>>>>();
        var behavior = new LoggingBehavior<CreateAdvertCommand, Result<Guid>>(loggerMock.Object);

        var expectedGuid = Guid.NewGuid();
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();
        nextMock.Setup(n => n(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedGuid));

        var request = new CreateAdvertRequest("Title", "Desc", 1000m, 50m, 2, 1, "Sale");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // 2. Act
        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedGuid, result.Value);
        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNextReturnsFailure_LogsWarningAndReturnsFailure()
    {
        // 1. Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<CreateAdvertCommand, Result<Guid>>>>();
        var behavior = new LoggingBehavior<CreateAdvertCommand, Result<Guid>>(loggerMock.Object);

        var error = Error.Validation("Validation.Failed", "Title is required.");
        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();
        nextMock.Setup(n => n(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(error));

        var request = new CreateAdvertRequest("", "Desc", 1000m, 50m, 2, 1, "Sale");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // 2. Act
        var result = await behavior.Handle(command, nextMock.Object, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Failed", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenNextThrows_LogsErrorAndRethrows()
    {
        // 1. Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<CreateAdvertCommand, Result<Guid>>>>();
        var behavior = new LoggingBehavior<CreateAdvertCommand, Result<Guid>>(loggerMock.Object);

        var nextMock = new Mock<RequestHandlerDelegate<Result<Guid>>>();
        nextMock.Setup(n => n(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB connection failed"));

        var request = new CreateAdvertRequest("Title", "Desc", 1000m, 50m, 2, 1, "Sale");
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // 2. Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(command, nextMock.Object, CancellationToken.None));
    }
}