using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Admin.DismissAdvertReport;

public class DismissAdvertReportCommandHandler(
    IAdvertReportRepository advertReportRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DismissAdvertReportCommand>
{
    public async Task<Result> Handle(DismissAdvertReportCommand command, CancellationToken ct)
    {
        var report = await advertReportRepository.GetByUuidAsync(command.ReportUuid, ct);

        if (report is null)
        {
            return Result.Failure(Error.NotFound("AdvertReport.NotFound", "Report not found."));
        }

        await advertReportRepository.DeleteAsync(report, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}