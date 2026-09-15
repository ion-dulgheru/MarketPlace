using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class UserSessionRepository(DataContext context) : IUserSessionRepository
{
    public async Task AddAsync(UserSession session, CancellationToken ct)
    {
        await context.UserSessions.AddAsync(session, ct);
    }

    public async Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct)
    {
        return await context.UserSessions
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, ct);
    }
}