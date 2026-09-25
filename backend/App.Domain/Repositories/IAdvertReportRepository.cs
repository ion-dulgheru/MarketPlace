using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IAdvertReportRepository
{
    Task<bool> ExistsAsync(Guid reporterUuid, long advertId, CancellationToken ct);
    Task AddAsync(AdvertReport report, CancellationToken ct);
    Task<IReadOnlyList<AdvertReport>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<AdvertReport?> GetByUuidAsync(Guid uuid, CancellationToken ct);
    Task DeleteAsync(AdvertReport report, CancellationToken ct);
}