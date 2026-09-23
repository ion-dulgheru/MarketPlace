using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class AdvertReportRepository(DataContext context) : IAdvertReportRepository
{
    public async Task<bool> ExistsAsync(Guid reporterUuid, long advertId, CancellationToken ct)
    {
        return await context.AdvertReports
            .AnyAsync(x => x.ReporterUuid == reporterUuid && x.AdvertId == advertId, ct);
    }

    public async Task AddAsync(AdvertReport report, CancellationToken ct)
    {
        await context.AdvertReports.AddAsync(report, ct);
    }
}