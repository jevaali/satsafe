using Base.Contracts;

namespace Base.Domain;

public abstract class BaseEntityWithMetaSoftDelete : BaseEntityWithMeta, IEntityMetaDelete
{
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}