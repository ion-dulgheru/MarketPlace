using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IFavoriteAdvertRepository
{
    Task<FavoriteAdvert?> GetAsync(Guid userUuid, long advertId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userUuid, long advertId, CancellationToken ct);
    Task<(IReadOnlyList<Advert> Items, int TotalCount)> GetFavoritesByUserAsync(Guid userUuid, int page, int pageSize, CancellationToken ct);
    Task AddAsync(FavoriteAdvert favorite, CancellationToken ct);
    void Remove(FavoriteAdvert favorite);
}
