using Xunit;

namespace App.Application.Tests.UseCases.Adverts.CreateAdvert;

public class CreateAdvertHandlerTests
{
    [Fact]
    public async Task Handle_WhenValidCommand_ShouldAddAdvertAndSave()
    {
        // 1. Arrange (Set up mocks and dependencies)
        var mockRepository = new Mock<IAdvertRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateAdvertHandler(mockRepository.Object, mockUnitOfWork.Object);

        var request = new CreateAdvertRequest(
            "Cozy Studio", 
            "Great apartment in city center", 
            50000m, 
            45.5m, 
            2, 
            3, 
            AdvertType.Sale
        );
        var command = new CreateAdvertCommand(request, Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify that repository.AddAsync and unitOfWork.SaveChangesAsync were each called exactly once
        mockRepository.Verify(x => x.AddAsync(It.IsAny<Advert>(), It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}