using App.Domain.Entities;
using App.Domain.Repositories;

namespace App.Persistence.Repositories;

public class ContactRequestRepository(DataContext context) : IContactRequestRepository
{
    public async Task AddAsync(ContactRequest contactRequest, CancellationToken ct)
    {
        await context.ContactRequests.AddAsync(contactRequest, ct);
    }
}