using App.Application.UseCases.Adverts.DeleteAdvert;
using App.Domain.Entities;
using App.Domain.Repositories;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.DeleteAdvert;

public class DeleteAdvertCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOwnerDeletesAdvert_SoftDeletesAndSaves()
    {
        var ownerUuid = Guid.NewGuid();
        var advert = Advert.Create(
            ownerUuid, "Apartment", "Description", 100000m, 60m, 2, 1,
            AdvertType.Sale, DateTime.UtcNow.AddDays(30));
        var repository = new Mock<IAdvertRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository
            .Setup(x => x.GetByUuidForOwnerAsync(advert.Guid, ownerUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var handler = new DeleteAdvertCommandHandler(repository.Object, unitOfWork.Object);

        var result = await handler.Handle(
            new DeleteAdvertCommand(advert.Guid, ownerUuid),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(advert.IsActive);
        Assert.NotNull(advert.DeletedAt);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}