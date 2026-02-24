using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Alert : BaseEntity
{
    // Foreign keys
    public Guid UserId { get; set; }
    public Guid ConjunctionEventId { get; set; }

    public string Severity { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }

    // Navigation properties
    public AppUser? User { get; set; }
    public ConjunctionEvent? ConjunctionEvent { get; set; }
}
