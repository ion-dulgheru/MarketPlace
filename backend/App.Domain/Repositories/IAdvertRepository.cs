using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IAdvertRepository
{
    Task<Advert?> GetByUuidAsync(Guid uuid, CancellationToken ct);
    Task<Advert?> GetByUuidForOwnerAsync(Guid uuid, Guid ownerUuid, CancellationToken ct);
    Task<IReadOnlyList<Advert>> GetActiveAsync(int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<Advert>> GetByUserAsync(Guid userUuid, int page, int pageSize, CancellationToken ct);
    Task AddAsync(Advert advert, CancellationToken ct);
}