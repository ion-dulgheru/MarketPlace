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
    public async Task Handle_WhenAdvertsExist_ReturnsMappedAdvertResponsesAndTotalCount()
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
            .Setup(x => x.GetActiveAsync(It.Is<AdvertSearchCriteria>(c => c.Page == 2 && c.PageSize == 10), It.IsAny<CancellationToken>()))
            .ReturnsAsync(([advert], 1));

        var handler = new GetActiveAdvertsCommandHandler(repository.Object);

        var result = await handler.Handle(
            new GetActiveAdvertsCommand(new GetAdvertsRequest(2, 10)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(1, result.Value.TotalCount);
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

        repository.Verify(x => x.GetActiveAsync(It.IsAny<AdvertSearchCriteria>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchAndFilters_PassesCorrectCriteriaToRepository()
    {
        var repository = new Mock<IAdvertRepository>();
        AdvertSearchCriteria? capturedCriteria = null;

        repository
            .Setup(x => x.GetActiveAsync(It.IsAny<AdvertSearchCriteria>(), It.IsAny<CancellationToken>()))
            .Callback<AdvertSearchCriteria, CancellationToken>((c, _) => capturedCriteria = c)
            .ReturnsAsync(([], 0));

        var handler = new GetActiveAdvertsCommandHandler(repository.Object);

        var request = new GetAdvertsRequest(
            Page: 1,
            PageSize: 15,
            SearchTerm: "penthouse",
            Type: "Sale",
            City: "Chisinau",
            MinPrice: 50000m,
            MaxPrice: 200000m,
            MinSurfaceArea: 60m,
            MaxSurfaceArea: 150m,
            Rooms: 3,
            SortBy: "price",
            SortDescending: false);

        var result = await handler.Handle(
            new GetActiveAdvertsCommand(request),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedCriteria);
        Assert.Equal(1, capturedCriteria.Page);
        Assert.Equal(15, capturedCriteria.PageSize);
        Assert.Equal("penthouse", capturedCriteria.SearchTerm);
        Assert.Equal(AdvertType.Sale, capturedCriteria.Type);
        Assert.Equal("Chisinau", capturedCriteria.City);
        Assert.Equal(50000m, capturedCriteria.MinPrice);
        Assert.Equal(200000m, capturedCriteria.MaxPrice);
        Assert.Equal(60m, capturedCriteria.MinSurfaceArea);
        Assert.Equal(150m, capturedCriteria.MaxSurfaceArea);
        Assert.Equal(3, capturedCriteria.Rooms);
        Assert.Equal("price", capturedCriteria.SortBy);
        Assert.False(capturedCriteria.SortDescending);
    }
}