using App.Application.Abstractions.Email;
using App.Application.UseCases.Adverts.ReportAdvert;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.ValueObjects;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Adverts.ReportAdvert;

public class ReportAdvertCommandHandlerTests
{
    private static Advert CreateSampleAdvert()
    {
        var address = Address.Create("Moldova", "Chisinau", "Centru", "Stefan cel Mare", "10").Value;
        return Advert.Create(
            Guid.NewGuid(), "Nice flat", "Description", 100000m, 60m, 2, 3,
            AdvertType.Sale, address, DateTime.UtcNow.AddDays(30));
    }

    [Fact]
    public async Task Handle_WhenReasonIsInvalid_ReturnsInvalidReason()
    {
        // 1. Arrange
        var advertRepositoryMock = new Mock<IAdvertRepository>();
        var advertReportRepositoryMock = new Mock<IAdvertReportRepository>();
        var emailSenderMock = new Mock<IEmailSender>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new ReportAdvertCommandHandler(
            advertRepositoryMock.Object, advertReportRepositoryMock.Object,
            emailSenderMock.Object, unitOfWorkMock.Object);

        var command = new ReportAdvertCommand(Guid.NewGuid(), "NotARealReason", Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("AdvertReport.InvalidReason", result.Error.Code);
        advertRepositoryMock.Verify(
            r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAdvertNotFound_ReturnsNotFound()
    {
        // 1. Arrange
        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Advert?)null);

        var advertReportRepositoryMock = new Mock<IAdvertReportRepository>();
        var emailSenderMock = new Mock<IEmailSender>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new ReportAdvertCommandHandler(
            advertRepositoryMock.Object, advertReportRepositoryMock.Object,
            emailSenderMock.Object, unitOfWorkMock.Object);

        var command = new ReportAdvertCommand(Guid.NewGuid(), "Spam", Guid.NewGuid());

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Advert.NotFound", result.Error.Code);
        advertReportRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<AdvertReport>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadyReportedByThisUser_ReturnsSuccessWithoutDuplicateRow()
    {
        // 1. Arrange
        var advert = CreateSampleAdvert();
        var reporterUuid = Guid.NewGuid();

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var advertReportRepositoryMock = new Mock<IAdvertReportRepository>();
        advertReportRepositoryMock
            .Setup(r => r.ExistsAsync(reporterUuid, advert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var emailSenderMock = new Mock<IEmailSender>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new ReportAdvertCommandHandler(
            advertRepositoryMock.Object, advertReportRepositoryMock.Object,
            emailSenderMock.Object, unitOfWorkMock.Object);

        var command = new ReportAdvertCommand(advert.Uuid, "Fraud", reporterUuid);

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        advertReportRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<AdvertReport>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        emailSenderMock.Verify(
            e => e.SendAdvertReportedEmailAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNewReport_SavesAndSendsNotification()
    {
        // 1. Arrange
        var advert = CreateSampleAdvert();
        var reporterUuid = Guid.NewGuid();

        var advertRepositoryMock = new Mock<IAdvertRepository>();
        advertRepositoryMock
            .Setup(r => r.GetByUuidAsync(advert.Uuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(advert);

        var advertReportRepositoryMock = new Mock<IAdvertReportRepository>();
        advertReportRepositoryMock
            .Setup(r => r.ExistsAsync(reporterUuid, advert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var emailSenderMock = new Mock<IEmailSender>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new ReportAdvertCommandHandler(
            advertRepositoryMock.Object, advertReportRepositoryMock.Object,
            emailSenderMock.Object, unitOfWorkMock.Object);

        var command = new ReportAdvertCommand(advert.Uuid, "Duplicate", reporterUuid);

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        advertReportRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<AdvertReport>(ar =>
                    ar.AdvertId == advert.Id &&
                    ar.ReporterUuid == reporterUuid &&
                    ar.Reason == AdvertReportReason.Duplicate),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        emailSenderMock.Verify(
            e => e.SendAdvertReportedEmailAsync(advert.Uuid, advert.Title, "Duplicate", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}