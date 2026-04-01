namespace Base.Contracts;

public interface IBaseEntityWithMetaSoftDelete : IBaseEntityWithMeta<Guid>
{
}

public interface IBaseEntityWithMetaSoftDelete<TKey> : IBaseEntity<TKey>, IBaseEntityMeta, IBaseEntitySoftDelete 
    where TKey : IEquatable<TKey>
{
    
}
