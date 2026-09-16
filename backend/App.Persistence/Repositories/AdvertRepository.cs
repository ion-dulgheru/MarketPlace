using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class AdvertRepository(DataContext context) : IAdvertRepository
{
    public async Task<Advert?> GetByUuidAsync(Guid uuid, CancellationToken ct)
    {
        return await context.Adverts.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Guid == uuid, ct);
    }

    public async Task<Advert?> GetByUuidForOwnerAsync(Guid uuid, Guid ownerUuid, CancellationToken ct)
    {
        return await context.Adverts.Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Guid == uuid && x.UserUuid == ownerUuid, ct);
    }

    public async Task<(IReadOnlyList<Advert> Items, int TotalCount)> GetActiveAsync(
        AdvertSearchCriteria criteria,
        CancellationToken ct)
    {
        var query = context.Adverts
            .Include(x => x.Photos)
            .Where(x => x.IsActive && x.Status == AdvertStatus.Active);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var term = criteria.SearchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(term) ||
                x.Description.ToLower().Contains(term));
        }

        if (criteria.Type.HasValue)
        {
            query = query.Where(x => x.Type == criteria.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.City))
        {
            var city = criteria.City.Trim().ToLower();
            query = query.Where(x => x.Address.City.ToLower().Contains(city));
        }

        if (criteria.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= criteria.MaxPrice.Value);
        }

        if (criteria.MinSurfaceArea.HasValue)
        {
            query = query.Where(x => x.SurfaceArea >= criteria.MinSurfaceArea.Value);
        }

        if (criteria.MaxSurfaceArea.HasValue)
        {
            query = query.Where(x => x.SurfaceArea <= criteria.MaxSurfaceArea.Value);
        }

        if (criteria.Rooms.HasValue)
        {
            query = query.Where(x => x.Rooms == criteria.Rooms.Value);
        }

        var totalCount = await query.CountAsync(ct);

        query = criteria.SortBy?.ToLower() switch
        {
            "price" => criteria.SortDescending
                ? query.OrderByDescending(x => x.Price).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.Price).ThenBy(x => x.Id),
            "surfacearea" => criteria.SortDescending
                ? query.OrderByDescending(x => x.SurfaceArea).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.SurfaceArea).ThenBy(x => x.Id),
            _ => criteria.SortDescending
                ? query.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.CreatedDate).ThenBy(x => x.Id)
        };

        var items = await query
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Advert>> GetByUserAsync(Guid userUuid, int page, int pageSize, CancellationToken ct)
    {
        return await context.Adverts.Include(x => x.Photos)
            .Where(x => x.UserUuid == userUuid)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Advert advert, CancellationToken ct)
    {
        await context.Adverts.AddAsync(advert, ct);
    }
}

