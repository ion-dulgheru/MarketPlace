using App.Application.Abstractions.Interfaces;
using App.Application.UseCases.Adverts.DeleteAdvertPhoto;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.DeleteAdvertPhoto;

public class DeleteAdvertPhotoCommandHandlerTests
{
    private readonly Mock<IAdvertRepository> _mockRepository;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeleteAdvertPhotoCommandHandler _handler;

    public DeleteAdvertPhotoCommandHandlerTests()
    {
        _mockRepository = new Mock<IAdvertRepository>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new DeleteAdvertPhotoCommandHandler(
            _mockRepository.Object,
            _mockFileStorage.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenAdvertNotFound_ShouldReturnNotFoundError()
    {
        var advertUuid = Guid.NewGuid();
        var photoUuid = Guid.NewGuid();
        var userUuid = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByUuidForOwnerAsync(advertUuid, userUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var command = new DeleteAdvertPhotoCommand(advertUuid, photoUuid, userUuid);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.NotFound.Code, result.Error.Code);
        _mockFileStorage.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPhotoNotFound_ShouldReturnPhotoNotFoundError()
    {
        var advertUuid = Guid.NewGuid();
        var photoUuid = Guid.NewGuid();
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

        var command = new DeleteAdvertPhotoCommand(advertUuid, photoUuid, userUuid);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.PhotoNotFound.Code, result.Error.Code);
        _mockFileStorage.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldRemovePhotoDeleteFileAndCommit()
    {
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

        var photo = AdvertPhoto.Create("/uploads/adverts/photo.jpg", true);
        advert.AddPhoto(photo);

        _mockRepository
            .Setup(x => x.GetByUuidForOwnerAsync(advertUuid, userUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var command = new DeleteAdvertPhotoCommand(advertUuid, photo.Guid, userUuid);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(advert.Photos);
        _mockFileStorage.Verify(x => x.DeleteFileAsync("/uploads/adverts/photo.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
