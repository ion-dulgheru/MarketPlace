using App.Application.UseCases.Adverts.GetAdvertContactRequests;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetAdvertContactRequests;

public class GetAdvertContactRequestsQueryHandlerTests
{
    private static Advert CreateSampleAdvert(Guid ownerUuid)
    {
        var address = Address.Create("Moldova", "Chisinau", "Centru", "Stefan cel Mare", "10").Value;
        return Advert.Create(ownerUuid, "Nice flat", "Description", 100000m, 60m, 2, 3, AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ReturnsNotFound()
    {
        // 1. Arrange
        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidForOwnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();

        var handler = new GetAdvertContactRequestsQueryHandler(
            advertRepositoryMock.Object, contactRequestRepositoryMock.Object);

        var query = new GetAdvertContactRequestsQuery(Guid.NewGuid(), Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(query, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Advert.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenOwner_ReturnsMappedContactRequests()
    {
        // 1. Arrange
        var ownerUuid = Guid.NewGuid();
        var advert = CreateSampleAdvert(ownerUuid);
        var contactRequest = ContactRequest.Create(advert.Uuid, Guid.NewGuid(), "Interested!");

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidForOwnerAsync(advert.Uuid, ownerUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetByAdvertUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContactRequest> { contactRequest });

        var handler = new GetAdvertContactRequestsQueryHandler(
            advertRepositoryMock.Object, contactRequestRepositoryMock.Object);

        var query = new GetAdvertContactRequestsQuery(advert.Uuid, ownerUuid);

        // 2. Act
        var result = await handler.Handle(query, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Interested!", result.Value[0].Message);
        Assert.Equal("Unread", result.Value[0].Status);
    }

    [Fact]
    public async Task Handle_WhenOwnerWithUserDetails_ReturnsMappedContactRequestsWithSenderInfo()
    {
        // 1. Arrange
        var ownerUuid = Guid.NewGuid();
        var advert = CreateSampleAdvert(ownerUuid);
        var senderUser = User.Create("buyer@example.com", "hash");
        var senderDetails = UserDetails.Create(senderUser.Id, "John", "Doe", null, "+37369123456");
        var contactRequest = ContactRequest.Create(advert.Uuid, senderUser.Guid, "Can I visit tomorrow?");

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidForOwnerAsync(advert.Uuid, ownerUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetByAdvertUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContactRequest> { contactRequest });

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByUuidAsync(senderUser.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(senderUser);
        userRepositoryMock
            .Setup(r => r.GetDetailsByUserIdAsync(senderUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(senderDetails);

        var handler = new GetAdvertContactRequestsQueryHandler(
            advertRepositoryMock.Object, contactRequestRepositoryMock.Object, userRepositoryMock.Object);

        var query = new GetAdvertContactRequestsQuery(advert.Uuid, ownerUuid);

        // 2. Act
        var result = await handler.Handle(query, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var first = result.Value[0];
        Assert.Equal("Can I visit tomorrow?", first.Message);
        Assert.Equal("buyer@example.com", first.SenderEmail);
        Assert.Equal("John Doe", first.SenderName);
        Assert.Equal("+37369123456", first.SenderPhone);
    }
}