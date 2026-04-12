using System.ComponentModel.DataAnnotations;

namespace App.Application.Features.Notifications.DTOs;

public class SaveNotificationsRequest
{
    [Range(0.1, 10000)]
    public double ConjunctionThresholdKm { get; set; } = 50.0;

    [Range(0.1, 10000)]
    public double AlertThresholdKm { get; set; } = 5.0;

    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}
