using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session, CancellationToken ct);

    Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct);

    Task<List<UserSession>> GetActiveByUserIdAsync(long userId, CancellationToken ct);
}