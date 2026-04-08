using App.Domain.Entities.Identity;
using Base.Domain;

namespace App.Domain.Entities;

public class NotificationSetting : BaseEntityWithMeta
{

    /// <summary>Miss distance (km) below which a conjunction event is recorded.</summary>
    public double ConjunctionThresholdKm { get; set; } = 50.0;

    /// <summary>Miss distance (km) below which an alert is generated for a recorded conjunction.</summary>
    public double AlertThresholdKm { get; set; } = 5.0;

    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }

    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}
