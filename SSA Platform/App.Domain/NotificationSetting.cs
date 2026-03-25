using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class NotificationSetting : BaseEntityWithMeta
{

    public double ThresholdDistance { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    
    public void UpdateThreshold(double distance)
    {
        if (distance <= 0)
            throw new ArgumentException("Threshold must be positive");

        ThresholdDistance = distance;
    }
}
