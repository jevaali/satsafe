using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class NotificationSettingIndexViewModel
{
    public IEnumerable<NotificationSettingListItemViewModel> Settings { get; set; } = [];
    public int TotalCount { get; set; }
}

public class NotificationSettingListItemViewModel
{
    public Guid   Id                    { get; set; }
    public string UserName              { get; set; } = string.Empty;
    public double ConjunctionThresholdKm { get; set; }
    public double AlertThresholdKm      { get; set; }
    public bool   EmailEnabled          { get; set; }
    public bool   SmsEnabled            { get; set; }
}

public class NotificationSettingDetailViewModel
{
    public Guid   Id                    { get; set; }
    public string UserName              { get; set; } = string.Empty;
    public string UserEmail             { get; set; } = string.Empty;
    public double ConjunctionThresholdKm { get; set; }
    public double AlertThresholdKm      { get; set; }
    public bool   EmailEnabled          { get; set; }
    public bool   SmsEnabled            { get; set; }
    public string?   CreatedBy          { get; set; }
    public DateTime? CreatedAt          { get; set; }
    public DateTime? UpdatedAt          { get; set; }
}

public class NotificationSettingFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "User")]
    public Guid AppUserId { get; set; }

    [Required, Range(0, 10000)]
    [Display(Name = "Conjunction Threshold (km)")]
    public double ConjunctionThresholdKm { get; set; } = 50;

    [Required, Range(0, 10000)]
    [Display(Name = "Alert Threshold (km)")]
    public double AlertThresholdKm { get; set; } = 5;

    [Display(Name = "Email Notifications")]
    public bool EmailEnabled { get; set; }

    [Display(Name = "SMS Notifications")]
    public bool SmsEnabled { get; set; }

    // Populated server-side
    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];
}

public class NotificationSettingDeleteViewModel
{
    public Guid   Id       { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double ConjunctionThresholdKm { get; set; }
}
