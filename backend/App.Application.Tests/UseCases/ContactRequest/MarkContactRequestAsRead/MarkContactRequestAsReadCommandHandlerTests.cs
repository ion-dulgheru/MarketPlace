using App.Application.UseCases.ContactRequests.MarkContactRequestAsRead;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.ContactRequests.MarkContactRequestAsRead;

public class MarkContactRequestAsReadCommandHandlerTests
{
    private static Advert CreateSampleAdvert(Guid ownerUuid)
    {
        var address = Address.Create("Moldova", "Chisinau", "Centru", "Stefan cel Mare", "10").Value;
        return Advert.Create(ownerUuid, "Nice flat", "Description", 100000m, 60m, 2, 3, AdvertType.Sale, address, DateTime.UtcNow.AddDays(30)); 
    }

    [Fact]
    public async Task Handle_WhenContactRequestNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContactRequest?)null);

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new MarkContactRequestAsReadCommandHandler(
            contactRequestRepositoryMock.Object, advertRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new MarkContactRequestAsReadCommand(Guid.NewGuid(), Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ContactRequest.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenCallerIsSender_MarksAsRead()
    {
        // 1. Arrange
        var ownerUuid = Guid.NewGuid();
        var senderUuid = Guid.NewGuid();
        var advert = CreateSampleAdvert(ownerUuid);
        var contactRequest = ContactRequest.Create(advert.Uuid, senderUuid, "Hello");

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetByUuidAsync(contactRequest.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contactRequest);

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new MarkContactRequestAsReadCommandHandler(
            contactRequestRepositoryMock.Object, advertRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new MarkContactRequestAsReadCommand(contactRequest.Uuid, senderUuid);

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ContactRequestStatus.Read, contactRequest.Status);
    }

    [Fact]
    public async Task Handle_WhenCallerIsOwner_MarksAsRead()
    {
        // 1. Arrange
        var ownerUuid = Guid.NewGuid();
        var advert = CreateSampleAdvert(ownerUuid);
        var contactRequest = ContactRequest.Create(advert.Uuid, Guid.NewGuid(), "Hello");

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetByUuidAsync(contactRequest.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contactRequest);

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new MarkContactRequestAsReadCommandHandler(
            contactRequestRepositoryMock.Object, advertRepositoryMock.Object, unitOfWorkMock.Object);

        var command = new MarkContactRequestAsReadCommand(contactRequest.Uuid, ownerUuid);

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ContactRequestStatus.Read, contactRequest.Status);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNeitherSenderNorOwner_ReturnsNotFound()
    {
        // 1. Arrange
        var advert = CreateSampleAdvert(Guid.NewGuid());
        var contactRequest = ContactRequest.Create(advert.Uuid, Guid.NewGuid(), "Hello");

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetByUuidAsync(contactRequest.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contactRequest);

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new MarkContactRequestAsReadCommandHandler(
            contactRequestRepositoryMock.Object, advertRepositoryMock.Object, unitOfWorkMock.Object);

        // A completely unrelated third user
        var command = new MarkContactRequestAsReadCommand(contactRequest.Uuid, Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ContactRequest.NotFound", result.Error.Code);
        Assert.Equal(ContactRequestStatus.Unread, contactRequest.Status); // unchanged
    }
}