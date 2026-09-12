namespace App.Domain.Common;

public abstract class PublicEntity : BaseEntity
{
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    // Alias for code that references the identifier as Uuid (e.g. repository queries, handler return values)
    public Guid Uuid
    {
        get => Guid;
        protected set => Guid = value;
    }
}
