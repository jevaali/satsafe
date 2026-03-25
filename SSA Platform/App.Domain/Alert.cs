using System.ComponentModel.DataAnnotations;
using App.Domain.Enums;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Alert : BaseEntityWithMeta
{
    [MaxLength(128)]
    public string? Message { get; set; }
    public EAlertSeverity Severity { get; set; }
    public bool IsAcknowledged { get; set; }
    
    public Guid UserId { get; set; }
    public virtual AppUser? User { get; set; }
    
    public Guid ConjunctionEventId { get; set; }
    public virtual ConjunctionEvent? ConjunctionEvent { get; set; }
}
