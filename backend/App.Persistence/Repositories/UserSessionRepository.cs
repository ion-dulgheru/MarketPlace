using App.Domain.Entities;
using App.Domain.Repositories;

namespace App.Persistence.Repositories;

public class UserSessionRepository(DataContext context) : IUserSessionRepository
{
    public async Task AddAsync(UserSession session, CancellationToken ct)
    {
        await context.UserSessions.AddAsync(session, ct);
    }
}