using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class NotificationSetting : BaseEntity
{

    public double ThresholdDistance { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}
