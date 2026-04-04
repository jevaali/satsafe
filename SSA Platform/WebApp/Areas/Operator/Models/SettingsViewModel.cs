using App.Application.DTO;
using App.Domain.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Areas.Operator.Models;

public class SettingsViewModel
{
    public SettingsDto Settings { get; set; } = null!;

    public UpdateOperatorForm OperatorForm { get; set; } = new();
    public AddHelperForm HelperForm { get; set; } = new();
    public NotificationForm NotificationForm { get; set; } = new();
}

public class UpdateOperatorForm
{
    [Required, MaxLength(124)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(124)]
    public string Country { get; set; } = string.Empty;

    public EOperatorType? OperatorType { get; set; }
}

public class AddHelperForm
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class NotificationForm
{
    [Range(0.1, 10000)]
    public double ThresholdDistance { get; set; } = 5.0;
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}
