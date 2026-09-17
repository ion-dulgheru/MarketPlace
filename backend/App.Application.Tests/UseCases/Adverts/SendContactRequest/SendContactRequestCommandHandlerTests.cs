using App.Application.UseCases.Adverts.SendContactRequest;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.SendContactRequest;

public class SendContactRequestCommandHandlerTests
{
    private static Advert CreateSampleAdvert(Guid ownerUuid, AdvertStatus status)
    {
        var address = Address.Create("Moldova", "Chisinau", "Centru", "Stefan cel Mare", "10").Value;
        var advert = Advert.Create(
            ownerUuid, "Nice flat", "Description", 100000m, 60m, 2, 3
            , AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));

        if (status != AdvertStatus.Active)
        {
            advert.ChangeStatus(status);
        }

        return advert;
    }

    [Fact]
    public async Task Handle_WhenAdvertNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new SendContactRequestCommandHandler(
            advertRepositoryMock.Object, contactRequestRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new SendContactRequestCommand(Guid.NewGuid(), "Hello?", Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Advert.NotFound", result.Error.Code);
        contactRequestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<ContactRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAdvertNotActive_ReturnsNotActive()
    {
        // 1. Arrange
        var advert = CreateSampleAdvert(Guid.NewGuid(), AdvertStatus.Sold);

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new SendContactRequestCommandHandler(
            advertRepositoryMock.Object, contactRequestRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new SendContactRequestCommand(advert.Uuid, "Hello?", Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Advert.NotActive", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenAdvertActive_CreatesContactRequestAndReturnsSuccess()
    {
        // 1. Arrange
        var advert = CreateSampleAdvert(Guid.NewGuid(), AdvertStatus.Active);
        var senderUuid = Guid.NewGuid();

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new SendContactRequestCommandHandler(
            advertRepositoryMock.Object, contactRequestRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new SendContactRequestCommand(advert.Uuid, "Is this still available?", senderUuid);

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        contactRequestRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<ContactRequest>(cr =>
                    cr.AdvertUuid == advert.Uuid &&
                    cr.FromUserUuid == senderUuid &&
                    cr.Message == "Is this still available?"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}