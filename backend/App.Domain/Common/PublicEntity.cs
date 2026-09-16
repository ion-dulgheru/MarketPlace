using System.ComponentModel.DataAnnotations.Schema;

namespace App.Domain.Common;

public abstract class PublicEntity : BaseEntity
{
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    [NotMapped]
    public Guid Uuid
    {
        get => Guid;
        protected set => Guid = value;
    }
}