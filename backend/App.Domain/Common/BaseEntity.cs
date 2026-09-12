namespace App.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; protected set; }
}