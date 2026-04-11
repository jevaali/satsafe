namespace App.Application.DTO;

public class NotificationSettingDto
{
    public double ConjunctionThresholdKm { get; init; }
    public double AlertThresholdKm { get; init; }
    public bool EmailEnabled { get; init; }
    public bool SmsEnabled { get; init; }
}
