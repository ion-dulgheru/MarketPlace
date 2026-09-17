using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IContactRequestRepository
{
    Task AddAsync(ContactRequest contactRequest, CancellationToken ct);
    Task<IReadOnlyList<ContactRequest>> GetByAdvertUuidAsync(Guid advertUuid, CancellationToken ct);
}