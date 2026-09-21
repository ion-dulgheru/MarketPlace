using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class ContactRequestRepository(DataContext context) : IContactRequestRepository
{
    public async Task AddAsync(ContactRequest contactRequest, CancellationToken ct)
    {
        await context.ContactRequests.AddAsync(contactRequest, ct);
    }

    public async Task<IReadOnlyList<ContactRequest>> GetByAdvertUuidAsync(Guid advertUuid, CancellationToken ct)
    {
        return await context.ContactRequests
            .Where(x => x.AdvertUuid == advertUuid)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<(ContactRequest Request, string AdvertTitle)>> GetReceivedByOwnerUserUuidAsync(Guid ownerUserUuid, CancellationToken ct)
    {
        var ownerAdverts = await context.Adverts
            .Where(a => a.UserUuid == ownerUserUuid)
            .Select(a => new { a.Uuid, a.Title })
            .ToListAsync(ct);

        var advertTitleMap = ownerAdverts.ToDictionary(a => a.Uuid, a => a.Title);
        var ownerAdvertUuids = advertTitleMap.Keys.ToList();

        var requests = await context.ContactRequests
            .Where(cr => ownerAdvertUuids.Contains(cr.AdvertUuid))
            .OrderByDescending(cr => cr.CreatedDate)
            .ToListAsync(ct);

        return requests
            .Select(r => (r, advertTitleMap.TryGetValue(r.AdvertUuid, out var title) ? title : string.Empty))
            .ToList();
    }

    public async Task<ContactRequest?> GetByUuidAsync(Guid uuid, CancellationToken ct)
    {
        return await context.ContactRequests.FirstOrDefaultAsync(x => x.Guid == uuid, ct);
    }
}