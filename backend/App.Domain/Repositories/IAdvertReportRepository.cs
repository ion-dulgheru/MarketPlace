using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IAdvertReportRepository
{
    Task<bool> ExistsAsync(Guid reporterUuid, long advertId, CancellationToken ct);
    Task AddAsync(AdvertReport report, CancellationToken ct);
}