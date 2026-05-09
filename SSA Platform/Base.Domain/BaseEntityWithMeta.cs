using Base.Contracts;

namespace Base.Domain;

public abstract class BaseEntityWithMeta : BaseEntity, IEntityMeta
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = default!;

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}