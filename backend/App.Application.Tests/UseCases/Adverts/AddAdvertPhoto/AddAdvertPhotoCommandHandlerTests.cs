using App.Application.Abstractions.Interfaces;
using App.Application.UseCases.Adverts.AddAdvertPhoto;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.AddAdvertPhoto;

public class AddAdvertPhotoCommandHandlerTests
{
    private readonly Mock<IAdvertRepository> _mockRepository;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AddAdvertPhotoCommandHandler _handler;

    public AddAdvertPhotoCommandHandlerTests()
    {
        _mockRepository = new Mock<IAdvertRepository>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new AddAdvertPhotoCommandHandler(
            _mockRepository.Object,
            _mockFileStorage.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenAdvertNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var advertUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByUuidForOwnerAsync(advertUuid, userUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        using var stream = new MemoryStream([1, 2, 3]);
        var command = new AddAdvertPhotoCommand(
            advertUuid,
            userUuid,
            stream,
            "photo.jpg",
            stream.Length,
            false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.NotFound.Code, result.Error.Code);
        _mockFileStorage.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldSaveFileAddPhotoAndCommit()
    {
        // Arrange
        var advertUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();
        var address = Address.Create("Moldova", "Chisinau", "Center", "Stefan cel Mare", "1").Value;
        var advert = Advert.Create(
            userUuid,
            "Title",
            "Description",
            100000m,
            75m,
            3,
            4,
            AdvertType.Sale,
            address,
            DateTime.UtcNow.AddDays(30));

        _mockRepository
            .Setup(x => x.GetByUuidForOwnerAsync(advertUuid, userUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        _mockFileStorage
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), "living_room.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/adverts/unique-file.jpg");

        using var stream = new MemoryStream([1, 2, 3, 4]);
        var command = new AddAdvertPhotoCommand(
            advertUuid,
            userUuid,
            stream,
            "living_room.jpg",
            stream.Length,
            true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Uuid);
        Assert.Equal("/uploads/adverts/unique-file.jpg", result.Value.PhotoUrl);
        Assert.True(result.Value.IsPrimary);
        Assert.Single(advert.Photos);
        Assert.Equal("/uploads/adverts/unique-file.jpg", advert.Photos.First().PhotoUrl);
        Assert.True(advert.Photos.First().IsPrimary);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
