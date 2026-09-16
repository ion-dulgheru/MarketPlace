using App.Domain.Entities;
using App.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence.Repositories;

public class UserRepository(DataContext context) : IUserRepository
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        return await context.Users.AnyAsync(x => x.Email == email, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct){
return await context.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
       await context.Users.AddAsync(user, ct);
    }
    public async Task<User?> GetByIdAsync(long id, CancellationToken ct)
{
    return await context.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
}
}