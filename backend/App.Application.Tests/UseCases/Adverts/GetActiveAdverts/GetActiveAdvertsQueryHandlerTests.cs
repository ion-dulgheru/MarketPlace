using App.Application.UseCases.Adverts.GetActiveAdverts;
using App.Contracts.Requests.Adverts;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetActiveAdverts;

public class GetActiveAdvertsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAdvertsExist_ReturnsMappedAdvertResponses()
    {
        var repository = new Mock<IAdvertRepository>();
        var address = Address.Create("USA", "New York", "NY", "5th Ave", "101").Value;
        var advert = Advert.Create(
            Guid.NewGuid(),
            "City apartment",
            "Central location",
            150000m,
            72m,
            3,
            2,
            AdvertType.Sale,
            address,
            DateTime.UtcNow.AddDays(30));

        var photo1 = AdvertPhoto.Create("url1", false);
        var photo2 = AdvertPhoto.Create("url2", true); // Primary
        advert.AddPhoto(photo1);
        advert.AddPhoto(photo2);

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
        Assert.NotNull(response.Address);
        Assert.Equal(address.Country, response.Address.Country);
        Assert.Equal(address.City, response.Address.City);
        Assert.Equal(address.Region, response.Address.Region);
        Assert.Equal(address.StreetAddress, response.Address.StreetAddress);
        Assert.Equal(address.StreetNumber, response.Address.StreetNumber);
        
        Assert.NotNull(response.Photos);
        var photoResponse = Assert.Single(response.Photos);
        Assert.Equal("url2", photoResponse.PhotoUrl);
        Assert.True(photoResponse.IsPrimary);

        repository.Verify(x => x.GetActiveAsync(2, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}