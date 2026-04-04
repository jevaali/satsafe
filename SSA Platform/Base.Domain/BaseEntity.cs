using Base.Contracts;

namespace Base.Domain;

public class BaseEntity : BaseEntity<Guid>, IBaseEntity
{
    public BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}

public abstract class BaseEntity<TKey> : IBaseEntity<TKey> 
    where TKey : IEquatable<TKey>
{
    public virtual Guid Id { get; set; }
}