using App.Domain.Entities.Identity;
using Base.Domain;

namespace App.Domain.Entities;

public class OperatorActionLog : BaseEntity
{
    public Guid OperatorId { get; set; }
    public virtual Operator? Operator { get; set; }

    public Guid UserId { get; set; }
    public virtual AppUser? User { get; set; }

    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
