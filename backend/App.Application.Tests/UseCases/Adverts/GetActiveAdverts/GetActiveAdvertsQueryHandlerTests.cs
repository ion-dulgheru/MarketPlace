using App.Application.UseCases.Adverts.GetActiveAdverts;
using App.Contracts.Requests.Adverts;
using App.Domain.Entities;
using App.Domain.Repositories;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetActiveAdverts;

public class GetActiveAdvertsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAdvertsExist_ReturnsMappedAdvertResponses()
    {
        var repository = new Mock<IAdvertRepository>();
        var advert = Advert.Create(
            Guid.NewGuid(),
            "City apartment",
            "Central location",
            150000m,
            72m,
            3,
            2,
            AdvertType.Sale,
            DateTime.UtcNow.AddDays(30));

        repository
            .Setup(x => x.GetActiveAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([advert]);

        var handler = new GetActiveAdvertsCommandHandler(repository.Object);

        var result = await handler.Handle(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(2, 10)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
        var response = Assert.Single(result.Value.Items);
        Assert.Equal(advert.Guid, response.Guid);
        Assert.Equal(advert.Title, response.Title);
        Assert.Equal(advert.Status.ToString(), response.Status);
        repository.Verify(x => x.GetActiveAsync(2, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}