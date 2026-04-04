namespace App.Application.DTO;

public class SaveNotificationsRequest
{
    public double ThresholdDistance { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}
