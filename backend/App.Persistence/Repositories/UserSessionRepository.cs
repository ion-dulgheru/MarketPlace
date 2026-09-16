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

    public Task<UserSession?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct)
    {
        return context.UserSessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash, ct);
    }
}