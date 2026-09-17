using App.Application.UseCases.Adverts.GetFavoriteAdverts;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetFavoriteAdverts;

public class GetFavoriteAdvertsQueryHandlerTests
{
    private readonly Mock<IFavoriteAdvertRepository> _favoriteRepository = new();
    private readonly GetFavoriteAdvertsQueryHandler _handler;

    public GetFavoriteAdvertsQueryHandlerTests()
    {
        _handler = new GetFavoriteAdvertsQueryHandler(_favoriteRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_ReturnsMappedFavoriteAdverts()
    {
        var userUuid = Guid.NewGuid();
        var address = Address.Create("Moldova", "Chișinău", "Centru", "Ștefan cel Mare", "1").Value;
        var advert = Advert.Create(
            Guid.NewGuid(), "Cozy Apartment", "Description", 100000m, 60m, 2, 1,
            AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));

        var advertsList = new List<Advert> { advert };

        _favoriteRepository
            .Setup(x => x.GetFavoritesByUserAsync(userUuid, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((advertsList, 1));

        var result = await _handler.Handle(
            new GetFavoriteAdvertsQuery(userUuid, 1, 20),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal(advert.Guid, result.Value.Items[0].Guid);
        Assert.Equal("Cozy Apartment", result.Value.Items[0].Title);
    }
}
