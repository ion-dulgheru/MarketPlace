using App.Application.UseCases.Adverts.GetAdvertById;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetAdvertById;

public class GetAdvertByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenAdvertExists_ReturnsAllDetailsAndAllPhotos()
    {
        // Arrange
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
            .Setup(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var handler = new GetAdvertByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetAdvertByIdQuery(advert.Guid), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value;
        Assert.Equal(advert.Guid, response.Guid);
        Assert.Equal("City apartment", response.Title);
        Assert.Equal("Central location", response.Description);
        Assert.Equal(150000m, response.Price);
        Assert.Equal(72m, response.SurfaceArea);
        Assert.Equal(3, response.Rooms);
        Assert.Equal(2, response.Floor);
        Assert.Equal(AdvertStatus.Active.ToString(), response.Status);
        Assert.Equal(AdvertType.Sale.ToString(), response.Type);
        Assert.NotNull(response.Address);
        Assert.Equal("USA", response.Address.Country);
        Assert.Equal("New York", response.Address.City);
        Assert.Equal("NY", response.Address.Region);
        Assert.Equal("5th Ave", response.Address.StreetAddress);
        Assert.Equal("101", response.Address.StreetNumber);

        Assert.NotNull(response.Photos);
        Assert.Equal(2, response.Photos.Count);
        // Primary photo should come first
        Assert.Equal("url2", response.Photos[0].PhotoUrl);
        Assert.True(response.Photos[0].IsPrimary);
        // Non-primary photo
        Assert.Equal("url1", response.Photos[1].PhotoUrl);
        Assert.False(response.Photos[1].IsPrimary);

        repository.Verify(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdvertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var repository = new Mock<IAdvertRepository>();
        var advertUuid = Guid.NewGuid();

        repository
            .Setup(x => x.GetByUuidAsync(advertUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var handler = new GetAdvertByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetAdvertByIdQuery(advertUuid), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.NotFound, result.Error);
        repository.Verify(x => x.GetByUuidAsync(advertUuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdvertUuidIsEmpty_ReturnsInvalidIdentifier()
    {
        // Arrange
        var repository = new Mock<IAdvertRepository>();
        var handler = new GetAdvertByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetAdvertByIdQuery(Guid.Empty), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.InvalidIdentifier, result.Error);
        repository.Verify(x => x.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
