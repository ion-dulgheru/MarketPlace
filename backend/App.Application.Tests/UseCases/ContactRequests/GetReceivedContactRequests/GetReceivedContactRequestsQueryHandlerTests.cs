using App.Application.UseCases.ContactRequests.GetReceivedContactRequests;
using App.Domain.Entities;
using App.Domain.Repositories;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.ContactRequests.GetReceivedContactRequests;

public class GetReceivedContactRequestsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCalled_ReturnsMappedReceivedContactRequestsWithAdvertTitle()
    {
        var ownerUuid = Guid.NewGuid();
        var advertUuid = Guid.NewGuid();
        var senderUser = User.Create("interested@example.com", "hash");
        var senderDetails = UserDetails.Create(senderUser.Id, "Jane", "Smith", null, "+37368000000");
        var contactRequest = ContactRequest.Create(advertUuid, senderUser.Guid, "Is this property still available?");

        var contactRequestRepositoryMock = new Mock<IContactRequestRepository>();
        contactRequestRepositoryMock
            .Setup(r => r.GetReceivedByOwnerUserUuidAsync(ownerUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(ContactRequest, string)> { (contactRequest, "Downtown Studio") });

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByUuidAsync(senderUser.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(senderUser);
        userRepositoryMock
            .Setup(r => r.GetDetailsByUserIdAsync(senderUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(senderDetails);

        var handler = new GetReceivedContactRequestsQueryHandler(
            contactRequestRepositoryMock.Object, userRepositoryMock.Object);

        var query = new GetReceivedContactRequestsQuery(ownerUuid);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        var item = result.Value[0];
        Assert.Equal("Is this property still available?", item.Message);
        Assert.Equal("interested@example.com", item.SenderEmail);
        Assert.Equal("Jane Smith", item.SenderName);
        Assert.Equal("+37368000000", item.SenderPhone);
        Assert.Equal(advertUuid, item.AdvertUuid);
        Assert.Equal("Downtown Studio", item.AdvertTitle);
    }
}
