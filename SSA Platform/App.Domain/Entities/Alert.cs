using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using App.Domain.Entities.Identity;
using Base.Domain;

namespace App.Domain.Entities;

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
    
    public void Acknowledge()
    {
        if (IsAcknowledged)
            return;

        IsAcknowledged = true;
    }
}
