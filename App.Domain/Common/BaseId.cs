namespace App.Domain.Common

public abstract class BaseEntity {
    
    public long Id {get; protected get; }
    public guid Uuid {get; protected get; } Guid.NewGuid();
    public bool IsActive {get; protected get;} = true;
    public DateTime CreatedAt {get; protected get; } = DateTime.UtcNow;
    public DateTime? UpdateAt {get; protected get;}
    
}

