using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Alert : BaseEntityWithMeta
{
    public string Severity { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    
    public Guid UserId { get; set; }
    public virtual AppUser? User { get; set; }
    
    public Guid ConjunctionEventId { get; set; }
    public virtual ConjunctionEvent? ConjunctionEvent { get; set; }
}
