namespace App.Domain.Common;

public abstract class BaseEntity
{
    public long Id {get; protected set; }
    public Guid Uuid {get; protected set; } = Guid.NewGuid();
    public bool IsActive {get; protected set; } = true;
    public DateTime CreatedAt {get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt {get; protected set; }
}