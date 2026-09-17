using App.Application.UseCases.Adverts.FavoriteAdvert;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;
using FavoriteAdvertEntity = App.Domain.Entities.FavoriteAdvert;

namespace App.Application.Tests.UseCases.Adverts.FavoriteAdvert;

public class FavoriteAdvertCommandHandlerTests
{
    private readonly Mock<IAdvertRepository> _advertRepository = new();
    private readonly Mock<IFavoriteAdvertRepository> _favoriteRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly FavoriteAdvertCommandHandler _handler;

    public FavoriteAdvertCommandHandlerTests()
    {
        _handler = new FavoriteAdvertCommandHandler(
            _advertRepository.Object,
            _favoriteRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenIsFavoriteTrue_AddsFavoriteAndSaves()
    {
        var ownerUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();
        var advert = CreateAdvert(ownerUuid);

        _advertRepository
            .Setup(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        _favoriteRepository
            .Setup(x => x.ExistsAsync(userUuid, advert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(
            new FavoriteAdvertCommand(advert.Guid, userUuid, IsFavorite: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _favoriteRepository.Verify(x => x.AddAsync(It.IsAny<FavoriteAdvertEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIsFavoriteFalse_RemovesFavoriteAndSaves()
    {
        var ownerUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();
        var advert = CreateAdvert(ownerUuid);
        var favorite = FavoriteAdvertEntity.Create(userUuid, advert.Id);

        _advertRepository
            .Setup(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        _favoriteRepository
            .Setup(x => x.GetAsync(userUuid, advert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorite);

        var result = await _handler.Handle(
            new FavoriteAdvertCommand(advert.Guid, userUuid, IsFavorite: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _favoriteRepository.Verify(x => x.Remove(favorite), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIsFavoriteFalseAndNotFavorited_ReturnsSuccessIdempotently()
    {
        var ownerUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();
        var advert = CreateAdvert(ownerUuid);

        _advertRepository
            .Setup(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        _favoriteRepository
            .Setup(x => x.GetAsync(userUuid, advert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FavoriteAdvertEntity?)null);

        var result = await _handler.Handle(
            new FavoriteAdvertCommand(advert.Guid, userUuid, IsFavorite: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _favoriteRepository.Verify(x => x.Remove(It.IsAny<FavoriteAdvertEntity>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAdvertNotFound_ReturnsNotFound()
    {
        var advertUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();

        _advertRepository
            .Setup(x => x.GetByUuidAsync(advertUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var result = await _handler.Handle(
            new FavoriteAdvertCommand(advertUuid, userUuid, IsFavorite: true),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUserIsOwnerAndIsFavoriteTrue_ReturnsCannotFavoriteOwnAdvert()
    {
        var ownerUuid = Guid.NewGuid();
        var advert = CreateAdvert(ownerUuid);

        _advertRepository
            .Setup(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var result = await _handler.Handle(
            new FavoriteAdvertCommand(advert.Guid, ownerUuid, IsFavorite: true),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.CannotFavoriteOwnAdvert.Code, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyFavoritedAndIsFavoriteTrue_ReturnsSuccessIdempotently()
    {
        var ownerUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();
        var advert = CreateAdvert(ownerUuid);

        _advertRepository
            .Setup(x => x.GetByUuidAsync(advert.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        _favoriteRepository
            .Setup(x => x.ExistsAsync(userUuid, advert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new FavoriteAdvertCommand(advert.Guid, userUuid, IsFavorite: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        _favoriteRepository.Verify(x => x.AddAsync(It.IsAny<FavoriteAdvertEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmptyGuid_ReturnsInvalidIdentifier()
    {
        var result = await _handler.Handle(
            new FavoriteAdvertCommand(Guid.Empty, Guid.NewGuid(), IsFavorite: true),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.InvalidIdentifier.Code, result.Error.Code);
    }

    private static Advert CreateAdvert(Guid ownerUuid)
    {
        var address = Address.Create("Moldova", "Chișinău", "Centru", "Ștefan cel Mare", "1").Value;
        return Advert.Create(
            ownerUuid, "Cozy Apartment", "Description", 100000m, 60m, 2, 1,
            AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));
    }
}
