using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Adverts;

namespace App.Application.UseCases.Admin.GetAdvertReports;

public record GetAdvertReportsQuery(int Page, int PageSize) : IQuery<IReadOnlyList<AdvertReportResponse>>;