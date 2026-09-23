using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Admin.GetAdvertReports;

public class GetAdvertReportsQueryHandler(
    IAdvertReportRepository advertReportRepository,
    IAdvertRepository advertRepository)
    : IQueryHandler<GetAdvertReportsQuery, IReadOnlyList<AdvertReportResponse>>
{
    public async Task<Result<IReadOnlyList<AdvertReportResponse>>> Handle(
        GetAdvertReportsQuery query, CancellationToken ct)
    {
        var reports = await advertReportRepository.GetAllAsync(query.Page, query.PageSize, ct);
        var result = new List<AdvertReportResponse>();

        foreach (var report in reports)
        {
            var advert = await advertRepository.GetByIdAsync(report.AdvertId, ct);

            result.Add(new AdvertReportResponse(
                report.Guid,
                advert?.Uuid ?? Guid.Empty,
                advert?.Title ?? "(deleted listing)",
                report.ReporterUuid,
                report.Reason.ToString(),
                report.Description,
                report.CreatedDate));
        }

        return Result.Success<IReadOnlyList<AdvertReportResponse>>(result);
    }
}