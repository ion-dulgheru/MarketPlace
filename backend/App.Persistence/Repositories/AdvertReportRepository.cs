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

    public async Task<IReadOnlyList<AdvertReport>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        return await context.AdvertReports
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<AdvertReport?> GetByUuidAsync(Guid uuid, CancellationToken ct)
    {
        return await context.AdvertReports.FirstOrDefaultAsync(x => x.Guid == uuid, ct); 
    }
    public Task DeleteAsync(AdvertReport report, CancellationToken ct)
    {
        context.AdvertReports.Remove(report);
        return Task.CompletedTask;
    }
}