using App.Application.Abstractions.Email;
using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Adverts.ReportAdvert;

public class ReportAdvertCommandHandler(
    IAdvertRepository advertRepository,
    IAdvertReportRepository advertReportRepository,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReportAdvertCommand>
{
    public async Task<Result> Handle(ReportAdvertCommand command, CancellationToken ct)
    {
        if (command.AdvertUuid == Guid.Empty)
        {
            return Result.Failure(AdvertErrors.InvalidIdentifier);
        }

        if (!Enum.TryParse<AdvertReportReason>(command.Reason, ignoreCase: true, out var reason))
        {
            return Result.Failure(AdvertReportErrors.InvalidReason);
        }

        var advert = await advertRepository.GetByUuidAsync(command.AdvertUuid, ct);
        if (advert is null)
        {
            return Result.Failure(AdvertErrors.NotFound);
        }

        var alreadyReported = await advertReportRepository.ExistsAsync(command.ReporterUuid, advert.Id, ct);
        if (alreadyReported)
        {
            return Result.Success();
        }

        var report = AdvertReport.Create(advert.Id, command.ReporterUuid, reason);
        await advertReportRepository.AddAsync(report, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await emailSender.SendAdvertReportedEmailAsync(advert.Uuid, advert.Title, reason.ToString(), ct);

        return Result.Success();
    }
}