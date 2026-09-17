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
    public async Task<ContactRequest?> GetByUuidAsync(Guid uuid, CancellationToken ct)
{
    return await context.ContactRequests.FirstOrDefaultAsync(x => x.Uuid == uuid, ct);
}
}