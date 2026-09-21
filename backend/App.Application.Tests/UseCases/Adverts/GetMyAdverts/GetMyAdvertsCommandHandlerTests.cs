using App.Application.UseCases.Adverts.GetMyAdverts;
using App.Contracts.Requests.Adverts;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.GetMyAdverts;

public class GetMyAdvertsCommandHandlerTests
{
    private readonly Mock<IAdvertRepository> _advertRepository = new();
    private readonly GetMyAdvertsCommandHandler _handler;

    public GetMyAdvertsCommandHandlerTests()
    {
        _handler = new GetMyAdvertsCommandHandler(_advertRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenCalled_ReturnsMappedAdvertsAndTotalCount()
    {
        var userUuid = Guid.NewGuid();
        var address = Address.Create("Moldova", "Chișinău", "Centru", "Ștefan cel Mare", "1").Value;
        var advert = Advert.Create(
            userUuid, "My Apartment", "Description", 150000m, 75m, 3, 2,
            AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));

        var advertsList = new List<Advert> { advert };

        _advertRepository
            .Setup(x => x.GetByUserAsync(userUuid, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((advertsList, 5));

        var request = new GetAdvertsRequest { Page = 1, PageSize = 10, Mine = true };
        var result = await _handler.Handle(
            new GetMyAdvertsCommand(request, userUuid),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal(advert.Guid, result.Value.Items[0].Guid);
        Assert.Equal("My Apartment", result.Value.Items[0].Title);
    }
}
