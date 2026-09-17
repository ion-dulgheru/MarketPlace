using App.Domain.Entities;

namespace App.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetByIdAsync(long id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task AddDetailsAsync(UserDetails details, CancellationToken ct);
}

