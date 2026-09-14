using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class AdvertRepository(DataContext context) : IAdvertRepository
{
    public async Task<Advert?> GetByUuidAsync(Guid uuid, CancellationToken ct)
    {
        return await context.Adverts.FirstOrDefaultAsync(x => x.Guid == uuid, ct);
    }

    public async Task<IReadOnlyList<Advert>> GetActiveAsync(int page, int pageSize, CancellationToken ct)
    {
        return await context.Adverts
            .Where(x => x.IsActive && x.Status == AdvertStatus.Active)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Advert>> GetByUserAsync(Guid userUuid, CancellationToken ct)
    {
        return await context.Adverts
            .Where(x => x.UserUuid == userUuid)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Advert advert, CancellationToken ct)
    {
        await context.Adverts.AddAsync(advert, ct);
    }

    public void Remove(Advert advert)
    {
        context.Adverts.Remove(advert);
    }
}

