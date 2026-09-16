using App.Application.UseCases.Adverts.UpdateAdvertStatus;
using App.Contracts.Requests.Adverts;
using App.Domain.Entities;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.UpdateAdvertStatus;

public class UpdateAdvertStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOwnerUpdatesStatus_ChangesStatusAndSaves()
    {
        var ownerUuid = Guid.NewGuid();
        var address = Address.Create("USA", "New York", "NY", "5th Ave", "101").Value;
        var advert = Advert.Create(
            ownerUuid, "Apartment", "Description", 100000m, 60m, 2, 1,
            AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));
        var repository = new Mock<IAdvertRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository
            .Setup(x => x.GetByUuidForOwnerAsync(advert.Guid, ownerUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var handler = new UpdateAdvertStatusCommandHandler(repository.Object, unitOfWork.Object);

        var result = await handler.Handle(
            new UpdateAdvertStatusCommand(
                advert.Guid,
                new UpdateAdvertStatusRequest("Sold"),
                ownerUuid),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AdvertStatus.Sold, advert.Status);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdvertIsNotOwned_ReturnsNotFoundAndDoesNotSave()
    {
        var repository = new Mock<IAdvertRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository
            .Setup(x => x.GetByUuidForOwnerAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var handler = new UpdateAdvertStatusCommandHandler(repository.Object, unitOfWork.Object);

        var result = await handler.Handle(
            new UpdateAdvertStatusCommand(
                Guid.NewGuid(),
                new UpdateAdvertStatusRequest("Sold"),
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(AdvertErrors.NotFound.Code, result.Error.Code);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}