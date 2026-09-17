using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class FavoriteAdvertRepository(DataContext context) : IFavoriteAdvertRepository
{
    public async Task<FavoriteAdvert?> GetAsync(Guid userUuid, long advertId, CancellationToken ct)
    {
        return await context.FavoriteAdverts
            .FirstOrDefaultAsync(x => x.UserUuid == userUuid && x.AdvertId == advertId, ct);
    }

    public async Task<bool> ExistsAsync(Guid userUuid, long advertId, CancellationToken ct)
    {
        return await context.FavoriteAdverts
            .AnyAsync(x => x.UserUuid == userUuid && x.AdvertId == advertId, ct);
    }

    public async Task<(IReadOnlyList<Advert> Items, int TotalCount)> GetFavoritesByUserAsync(
        Guid userUuid,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var favoriteAdvertIds = context.FavoriteAdverts
            .Where(f => f.UserUuid == userUuid)
            .Select(f => f.AdvertId);

        var query = context.Adverts
            .Include(x => x.Photos)
            .Where(x => favoriteAdvertIds.Contains(x.Id) && x.IsActive && x.Status == AdvertStatus.Active)
            .OrderByDescending(x => x.CreatedDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(FavoriteAdvert favorite, CancellationToken ct)
    {
        await context.FavoriteAdverts.AddAsync(favorite, ct);
    }

    public void Remove(FavoriteAdvert favorite)
    {
        context.FavoriteAdverts.Remove(favorite);
    }
}
