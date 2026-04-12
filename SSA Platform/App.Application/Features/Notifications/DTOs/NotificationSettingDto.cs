namespace App.Application.Features.Notifications.DTOs;

public class NotificationSettingDto
{
    public double ConjunctionThresholdKm { get; init; }
    public double AlertThresholdKm { get; init; }
    public bool EmailEnabled { get; init; }
    public bool SmsEnabled { get; init; }
}
