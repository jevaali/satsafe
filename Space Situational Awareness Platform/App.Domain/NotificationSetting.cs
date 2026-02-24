using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class NotificationSetting : BaseEntity
{
    // Foreign key
    public Guid UserId { get; set; }

    public double ThresholdDistance { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }

    // Navigation properties
    public AppUser? User { get; set; }
}
