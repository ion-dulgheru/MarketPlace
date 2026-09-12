namespace App.Domain.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct);
}