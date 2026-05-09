using Base.Contracts;

namespace Base.Domain;

public abstract class BaseEntityAppUser : BaseEntity, IAppUserId
{
    public Guid AppUserId { get; set; }
}