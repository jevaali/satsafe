using Base.Contracts;

namespace Base.Domain;

public abstract class BaseEntityWithMetaSoftDelete :BaseEntityWithMetaSoftDelete<Guid>,  IBaseEntityWithMetaSoftDelete
{
}

public abstract class BaseEntityWithMetaSoftDelete<TKey> : BaseEntityWithMeta<TKey>, IBaseEntityWithMetaSoftDelete<TKey>
    where TKey : IEquatable<TKey>
{
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}