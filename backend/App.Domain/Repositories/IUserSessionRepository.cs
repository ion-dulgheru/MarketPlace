using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session, CancellationToken ct);
}