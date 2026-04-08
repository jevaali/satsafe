namespace App.Application.DTO;

public class SaveNotificationsRequest
{
    public double ConjunctionThresholdKm { get; set; }
    public double AlertThresholdKm { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}
