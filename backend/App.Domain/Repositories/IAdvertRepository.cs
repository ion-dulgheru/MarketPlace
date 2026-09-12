using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IAdvertRepository
{
    Task<Advert?> GetByUuidAsync(Guid uuid, CancellationToken ct);
    Task<IReadOnlyList<Advert>> GetByUserAsync(Guid userUuid, CancellationToken ct); // or long, per the decision above
    Task AddAsync(Advert advert, CancellationToken ct);
    void Remove(Advert advert);
}